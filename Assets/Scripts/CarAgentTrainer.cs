using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoodHub.Core.Runtime;
using GoodHub.Core.Runtime.Observables;
using UnityEngine;

public class CarAgentTrainer : SceneSingleton<CarAgentTrainer>
{
    [SerializeField] private TrainingParameters _parameters;
    [SerializeField] private GameObject _carAgentPrefab;
    [SerializeField] private int _maxGenerations = 9999;
    [SerializeField] private int _stagnantGenerationThreshold = 540;

    private Dictionary<AgentTracker, CarAgent> _agentsAndTrackers;

    private AgentCollection _agentCollection;
    private int _generationIndex;

    private int _stagnantGenerationCountdown;
    private bool _stagnated;

    private ITrainingEnvironment _activeTrainingEnvironment;

    public bool YieldEveryFrame;
    public bool YieldEveryAgent;

    public ObservableBool OnlyShowBestAgents = false;

    private Dictionary<ITrainingEnvironment, List<Vector2>> _fitnessData = new Dictionary<ITrainingEnvironment, List<Vector2>>();

    private List<AgentTracker> _lastGenerationStandings = new List<AgentTracker>();

    private CarAgent _currentBestAgent;

    public event Action OnFitnessDataUpdated;

    public List<Vector2> ActiveEnvironmentFitnessData
    {
        get
        {
            if (_activeTrainingEnvironment == null)
                return new List<Vector2>();

            if (_fitnessData.TryGetValue(_activeTrainingEnvironment, out List<Vector2> fitnessData))
                return fitnessData;

            return new List<Vector2>();
        }
    }

    public int StagnantGenerationCountdown => _stagnantGenerationCountdown;

    private void Start()
    {
        _agentCollection = new AgentCollection(_parameters);

        SpawnCarAgents();

        _stagnantGenerationCountdown = _stagnantGenerationThreshold;

        OnlyShowBestAgents.OnValueChanged += UpdateAgentsVisibility;
    }

    private void SpawnCarAgents()
    {
        _activeTrainingEnvironment = TrackManager.Singleton.CurrentTrack;
        Transform startTransform = ((TrackCircuit) _activeTrainingEnvironment).StartTransform;

        _agentsAndTrackers = new Dictionary<AgentTracker, CarAgent>();

        int depthIndex = 1;
        foreach (AgentTracker agentTracker in _agentCollection.AgentTrackers)
        {
            CarAgent carAgent = Instantiate(_carAgentPrefab, startTransform.position, Quaternion.identity, transform).GetComponent<CarAgent>();
            carAgent.gameObject.name = $"{_carAgentPrefab.name}_{_agentsAndTrackers.Count}";

            carAgent.transform.position = startTransform.position;
            carAgent.transform.rotation = startTransform.rotation;

            carAgent.ResetAgent();
            carAgent.InitialiseGraphics(agentTracker.perceptron.Seed, depthIndex);

            _agentsAndTrackers.Add(agentTracker, carAgent);

            depthIndex++;
        }

        StartCoroutine(TrainingCoroutine());
    }

    private IEnumerator TrainingCoroutine()
    {
        yield return null;

        while (_generationIndex < _maxGenerations)
        {
            TrackCircuit trainingEnvironment = TrackManager.Singleton.CurrentTrack;

            if (_fitnessData.ContainsKey(trainingEnvironment) == false)
            {
                _fitnessData.Add(trainingEnvironment, new List<Vector2> {new Vector2(0, 0f)});
            }

            HashSet<AgentTracker> completedTrackers = new HashSet<AgentTracker>();
            List<AgentTracker> finishedTrackers = new List<AgentTracker>();

            foreach (KeyValuePair<AgentTracker, CarAgent> carTrackerPair in _agentsAndTrackers)
            {
                carTrackerPair.Value.SetBestAgent(carTrackerPair.Value == _currentBestAgent);
            }

            while (completedTrackers.Count < _agentsAndTrackers.Count)
            {
                // Update the perceptrons of all active agents
                foreach (KeyValuePair<AgentTracker, CarAgent> carTrackerPair in _agentsAndTrackers)
                {
                    carTrackerPair.Deconstruct(out AgentTracker tracker, out CarAgent carAgent);

                    if (completedTrackers.Contains(tracker))
                        continue;

                    if (carAgent.State is CarAgent.AgentState.Crashed or CarAgent.AgentState.Timeout or CarAgent.AgentState.Finished)
                    {
                        completedTrackers.Add(tracker);

                        carAgent.HandleAgentCompleted();

                        if (carAgent.State == CarAgent.AgentState.Finished)
                            finishedTrackers.Add(tracker);

                        if (YieldEveryAgent)
                            yield return null;

                        continue;
                    }

                    List<float> inputActivations = new List<float>();

                    inputActivations.AddRange(carAgent.GetViewRayDistances());
                    inputActivations.Add(carAgent.ThrottleInput);
                    inputActivations.Add(carAgent.BrakingInput);
                    inputActivations.Add(carAgent.SteeringInput);
                    inputActivations.Add(carAgent.SpeedNormalised);
                    inputActivations.Add(carAgent.TurningNormalised);

                    float[] outputActivations = tracker.perceptron.ProcessInputsToOutputs(inputActivations.ToArray());

                    carAgent.SetControls(outputActivations[0], outputActivations[1], outputActivations[2]);

                    carAgent.UpdateWithTime(Time.fixedDeltaTime);

                    tracker.fitness = carAgent.TrackProgress;
                }

                Physics2D.SyncTransforms();
                Physics2D.Simulate(Time.fixedDeltaTime);

                if (YieldEveryFrame)
                    yield return new WaitForFixedUpdate();
                //yield return null;
            }

            yield return null;

            // TRAINING SESSION FINISHED - All agents have completed so time for a new generation

            // Give each agent that finishes a bonus based on their position to make then go round the track faster
            for (int i = 0; i < finishedTrackers.Count; i++)
            {
                CarAgent carAgent = _agentsAndTrackers.First(item => item.Key == finishedTrackers[i]).Value;
                float timeAlive = carAgent.TimeAlive;
                float penalty = carAgent.Penalty;
                finishedTrackers[i].fitness = 100 + (50 - timeAlive) - penalty;
            }

            float maxFitness = 0;
            foreach (KeyValuePair<AgentTracker, CarAgent> carTrackerPair in _agentsAndTrackers)
            {
                if (carTrackerPair.Key.fitness < maxFitness)
                    continue;

                maxFitness = carTrackerPair.Key.fitness;
                _currentBestAgent = carTrackerPair.Value;
            }

            // Check if the simulation has stagnated
            if (maxFitness <= _fitnessData[trainingEnvironment][^1].y)
                _stagnantGenerationCountdown--;
            else
                _stagnantGenerationCountdown = _stagnantGenerationThreshold;

            _stagnated = _stagnantGenerationCountdown <= 0;

            // Log the best fitness text
            _generationIndex++;

            if (_stagnated)
            {
                TrackManager.Singleton.IncrementTrack();
                _stagnantGenerationCountdown = _stagnantGenerationThreshold;
            }

            // Log the best fitness for this training environment

            _fitnessData[trainingEnvironment].Add(new Vector2(_generationIndex, maxFitness));
            OnFitnessDataUpdated?.Invoke();

            _lastGenerationStandings = _agentCollection.AgentTrackers.OrderByDescending(agent => agent.fitness).ToList();

            // Cull and repopulate
            _agentCollection.ApplySurvivalCurve();
            _agentCollection.Repopulate();

            if (_stagnated)
            {
                _activeTrainingEnvironment = trainingEnvironment = TrackManager.Singleton.CurrentTrack;
                _stagnated = false;
            }

            // Reset all the cars
            foreach (KeyValuePair<AgentTracker, CarAgent> carTrackerPair in _agentsAndTrackers)
            {
                carTrackerPair.Deconstruct(out AgentTracker _, out CarAgent carAgent);

                carAgent.transform.position = trainingEnvironment.StartTransform.position;
                carAgent.transform.rotation = trainingEnvironment.StartTransform.rotation;

                carAgent.ResetAgent();
            }

            UpdateAgentsVisibility(OnlyShowBestAgents);
        }
    }

    private void UpdateAgentsVisibility(bool onlyShowBestAgents)
    {
        // Get the fitness of the third-best agent then use it to show all
        // agents at or above that fitness, i.e. the top 3 performers
        // Feels slightly more efficient than a list contains check

        if (onlyShowBestAgents)
        {
            // float lowerBoundFitness = _agentsAndTrackers
            //     .Keys
            //     .OrderByDescending(tracker => tracker.fitness)
            //     .Take(3)
            //     .Last().fitness;
            //
            // foreach (AgentTracker agentTracker in _agentsAndTrackers.Keys)
            // {
            //     _agentsAndTrackers[agentTracker].Graphics.SetVisible(agentTracker.fitness >= lowerBoundFitness);
            // }

            List<AgentTracker> bestAgents = _lastGenerationStandings.Take(Mathf.Clamp(_parameters.PopulationCount / 10, 1, 10)).ToList();

            foreach (AgentTracker agentTracker in _agentsAndTrackers.Keys)
            {
                _agentsAndTrackers[agentTracker].Graphics.SetVisible(bestAgents.Contains(agentTracker));
            }
        }
        else
        {
            foreach (AgentTracker agentTracker in _agentsAndTrackers.Keys)
            {
                _agentsAndTrackers[agentTracker].Graphics.SetVisible(true);
            }
        }
    }
}