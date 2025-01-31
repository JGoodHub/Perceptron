using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GoodHub.Core.Runtime;
using GoodHub.Core.Runtime.Observables;
using UnityEngine;

public class CarAgentTrainer : SceneSingleton<CarAgentTrainer>
{
    [SerializeField] private TrainingParameters _parameters;
    [SerializeField] private GameObject _carAgentPrefab;
    [SerializeField] private int _maxGenerations = 9999;
    [SerializeField] private int _stagnantGenerationThreshold = 50;

    private AgentsPool<CarAgent> _agentsPool;
    private int _generationIndex;

    private int _stagnantGenerationCountdown;
    private bool _stagnated;

    private ITrainingEnvironment _activeTrainingEnvironment;

    public bool YieldEveryFrame;
    public bool YieldEveryAgent;

    public ObservableBool OnlyShowBestAgents = false;

    private Dictionary<ITrainingEnvironment, List<Vector2>> _fitnessData = new Dictionary<ITrainingEnvironment, List<Vector2>>();

    private List<AgentEntity<CarAgent>> _lastGenerationStandings = new List<AgentEntity<CarAgent>>();

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
        OnlyShowBestAgents.OnValueChanged += UpdateAgentsVisibility;

        _stagnantGenerationCountdown = _stagnantGenerationThreshold;

        _agentsPool = new AgentsPool<CarAgent>(_parameters);

        SpawnCarAgents();

        TrainingTask().Forget();
    }

    private void SpawnCarAgents()
    {
        _activeTrainingEnvironment = TrackManager.Singleton.CurrentTrack;
        Transform startTransform = ((TrackCircuit) _activeTrainingEnvironment).StartTransform;

        int depthIndex = 1;
        foreach (AgentEntity<CarAgent> agentEntity in _agentsPool.AgentEntities)
        {
            CarAgent carAgent = Instantiate(_carAgentPrefab, startTransform.position, Quaternion.identity, transform).GetComponent<CarAgent>();
            carAgent.gameObject.name = $"{_carAgentPrefab.name}_{depthIndex}";

            carAgent.transform.position = startTransform.position;
            carAgent.transform.rotation = startTransform.rotation;

            carAgent.ResetBody();
            carAgent.InitialiseGraphics(agentEntity.Perceptron.Seed, depthIndex);

            agentEntity.AssignBody(carAgent);

            depthIndex++;
        }
    }

    private async UniTask TrainingTask()
    {
        await UniTask.WaitForFixedUpdate();

        while (_generationIndex < _maxGenerations)
        {
            TrackCircuit trainingEnvironment = TrackManager.Singleton.CurrentTrack;

            if (_fitnessData.ContainsKey(trainingEnvironment) == false)
            {
                _fitnessData.Add(trainingEnvironment, new List<Vector2> {new Vector2(0, 0f)});
            }

            HashSet<AgentEntity<CarAgent>> completedTrackers = new HashSet<AgentEntity<CarAgent>>();

            foreach (AgentEntity<CarAgent> agentEntity in _agentsPool.AgentEntities)
            {
                agentEntity.AgentBody.SetBestAgent(agentEntity.AgentBody == _currentBestAgent);
            }

            while (completedTrackers.Count < _agentsPool.AgentEntities.Count)
            {
                //float agentsUpdateStarTimestamp = Time.realtimeSinceStartup;

                bool agentCompletedThisFrame = false;

                // Update the perceptrons of all active agents
                foreach (AgentEntity<CarAgent> agentEntity in _agentsPool.AgentEntities)
                {
                    if (completedTrackers.Contains(agentEntity))
                        continue;

                    if (agentEntity.AgentBody.State != CarAgent.AgentState.Alive)
                    {
                        completedTrackers.Add(agentEntity);
                        agentCompletedThisFrame = true;

                        agentEntity.AgentBody.HandleAgentCompleted();

                        continue;
                    }

                    agentEntity.UpdateBrain();

                    agentEntity.AgentBody.UpdateWithTime(Time.fixedDeltaTime);
                }

                //Debug.Log($"{(Time.realtimeSinceStartup - agentsUpdateStarTimestamp) * 1000f}ms");

                Physics2D.SyncTransforms();
                Physics2D.Simulate(Time.fixedDeltaTime);

                if (YieldEveryFrame || (YieldEveryAgent && agentCompletedThisFrame))
                    await UniTask.DelayFrame(1, PlayerLoopTiming.FixedUpdate);
            }

            await UniTask.DelayFrame(5);

            // TRAINING SESSION FINISHED - All agents have completed so time for a new generation

            float maxFitness = 0;
            foreach (AgentEntity<CarAgent> agentEntity in _agentsPool.AgentEntities)
            {
                if (agentEntity.Fitness < maxFitness)
                    continue;

                maxFitness = agentEntity.Fitness;
                _currentBestAgent = agentEntity.AgentBody;
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

            _fitnessData[trainingEnvironment].Add(new Vector2(_fitnessData[trainingEnvironment].Count + 1, maxFitness));
            OnFitnessDataUpdated?.Invoke();

            _lastGenerationStandings = _agentsPool.AgentEntities.OrderByDescending(agent => agent.Fitness).ToList();

            // Cull and repopulate
            _agentsPool.ApplySurvivalCurve();
            _agentsPool.Repopulate(_generationIndex);

            if (_stagnated)
            {
                _activeTrainingEnvironment = trainingEnvironment = TrackManager.Singleton.CurrentTrack;
                _stagnated = false;
            }

            // Reset all the cars
            foreach (AgentEntity<CarAgent> agentEntity in _agentsPool.AgentEntities)
            {
                agentEntity.AgentBody.transform.position = trainingEnvironment.StartTransform.position;
                agentEntity.AgentBody.transform.rotation = trainingEnvironment.StartTransform.rotation;

                agentEntity.AgentBody.ResetBody();
            }

            UpdateAgentsVisibility(OnlyShowBestAgents);
        }
    }

    private void UpdateAgentsVisibility(bool onlyShowBestAgents)
    {
        if (onlyShowBestAgents)
        {
            List<AgentEntity<CarAgent>> bestAgents = _lastGenerationStandings.Take(Mathf.Clamp(_parameters.PopulationCount / 10, 1, 10)).ToList();

            foreach (AgentEntity<CarAgent> agentEntity in _agentsPool.AgentEntities)
            {
                agentEntity.AgentBody.Graphics.SetVisible(bestAgents.Contains(agentEntity));
            }
        }
        else
        {
            foreach (AgentEntity<CarAgent> agentEntity in _agentsPool.AgentEntities)
            {
                agentEntity.AgentBody.Graphics.SetVisible(true);
            }
        }
    }
}