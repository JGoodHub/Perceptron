using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NeuralNet;
using UnityEngine;
using Random = System.Random;

public class CarAgentTrainer : MonoBehaviour
{
    [SerializeField] private TrainingParameters _parameters;
    [SerializeField] private GameObject _carAgentPrefab;
    [SerializeField] private float _finishingPositionBonus;
    [SerializeField] private GraphElement _graph;
    [SerializeField] private int _maxGenerations = 999;
    [SerializeField] private int _stagnantGenerationThreshold = 25;

    private Dictionary<AgentTracker, CarAgent> _agentsAndTrackers;

    private AgentCollection _agentCollection;
    private int _generationIndex;

    private List<float> _bestFitnessPerGeneration = new List<float>();

    private int _stagnantGenerationCountdown;
    private bool _stagnated;

    public bool YieldEveryFrame;
    public bool YieldOnComplete;

    private void Start()
    {
        _agentCollection = new AgentCollection(_parameters);

        SpawnCarAgents();

        _stagnantGenerationCountdown = _stagnantGenerationThreshold;
    }

    private void SpawnCarAgents()
    {
        _bestFitnessPerGeneration.Add(0f);

        _agentsAndTrackers = new Dictionary<AgentTracker, CarAgent>();

        foreach (AgentTracker agentTracker in _agentCollection.AgentTrackers)
        {
            CarAgent carAgent = Instantiate(_carAgentPrefab, TrackManager.Instance.CurrentTrack.StartTransform.position, Quaternion.identity, transform).GetComponent<CarAgent>();
            carAgent.gameObject.name = $"{_carAgentPrefab.name}_{_agentsAndTrackers.Count}";

            carAgent.transform.position = TrackManager.Instance.CurrentTrack.StartTransform.position;
            carAgent.transform.rotation = TrackManager.Instance.CurrentTrack.StartTransform.rotation;

            carAgent.ResetAgent();
            carAgent.InitialiseGraphics(agentTracker.perceptron.Seed);

            _agentsAndTrackers.Add(agentTracker, carAgent);
        }

        UserInterface.Instance.UpdateText(0, 0);
        _graph.Initialise("Fitness Graph", "Generation", 10, "Fitness", 10);

        StartCoroutine(TrainingCoroutine());
    }

    private IEnumerator TrainingCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (_generationIndex < _maxGenerations)
        {
            HashSet<AgentTracker> completedTrackers = new HashSet<AgentTracker>();
            List<AgentTracker> finishedTrackers = new List<AgentTracker>();

            while (completedTrackers.Count < _agentsAndTrackers.Count)
            {
                foreach (KeyValuePair<AgentTracker, CarAgent> carTrackerPair in _agentsAndTrackers)
                {
                    carTrackerPair.Deconstruct(out AgentTracker tracker, out CarAgent carAgent);

                    if (completedTrackers.Contains(tracker))
                        continue;

                    if (carAgent.State is CarAgent.AgentState.Crashed or CarAgent.AgentState.Timeout or CarAgent.AgentState.Finished)
                    {
                        completedTrackers.Add(tracker);

                        if (carAgent.State == CarAgent.AgentState.Finished)
                            finishedTrackers.Add(tracker);

                        if (YieldOnComplete)
                            yield return null;

                        continue;
                    }

                    List<float> inputActivations = new List<float>();

                    inputActivations.AddRange(carAgent.GetViewRayDistances());
                    inputActivations.Add(carAgent.SteeringInput);
                    inputActivations.Add(carAgent.SpeedNormalised);

                    tracker.perceptron.SetInputActivations(inputActivations.ToArray());

                    tracker.perceptron.ProcessInputActivations();

                    float[] outputActivations = tracker.perceptron.GetOutputActivations();

                    carAgent.SetSteering(outputActivations[0]);
                    carAgent.SetThrottle(outputActivations[1]);

                    carAgent.UpdateWithTime(0.015f);

                    tracker.fitness = carAgent.TrackProgress;
                }

                Physics2D.SyncTransforms();
                Physics2D.Simulate(0.02f);

                if (YieldEveryFrame)
                    yield return new WaitForFixedUpdate();
            }

            yield return new WaitForSeconds(0.1f);

            // TRAINING SESSION FINISHED - All agents have completed so time for a new generation
            
            // Give each agent that finishes a bonus based on their position to make then go round the track faster
            for (int i = 0; i < finishedTrackers.Count; i++)
            {
                finishedTrackers[i].fitness += _finishingPositionBonus / (i + 1);
            }

            float maxFitness = _agentCollection.AgentTrackers.Max(tracker => tracker.fitness);

            // Check if the simulation has stagnated
            if (_generationIndex > 20 && maxFitness <= _bestFitnessPerGeneration[^1])
                _stagnantGenerationCountdown--;
            else
                _stagnantGenerationCountdown = _stagnantGenerationThreshold;

            _stagnated = _stagnantGenerationCountdown <= 0;

            // Log the best fitness text
            _bestFitnessPerGeneration.Add(maxFitness);
            _generationIndex++;

            if (_stagnated)
            {
                TrackManager.Instance.IncrementTrack();
                _stagnantGenerationCountdown = _stagnantGenerationThreshold;
                _stagnated = false;
            }

            // Log the best fitness graph
            List<Vector2> fitnessData = new List<Vector2>();
            for (int i = 0; i < _bestFitnessPerGeneration.Count; i++)
                fitnessData.Add(new Vector2(i, _bestFitnessPerGeneration[i]));

            _graph.SetBodyData(fitnessData);

            UserInterface.Instance.UpdateText(_generationIndex, maxFitness);

            // Cull and repopulate
            _agentCollection.ApplySurvivalCurve();
            _agentCollection.Repopulate();

            // Reset all the cars
            foreach (KeyValuePair<AgentTracker, CarAgent> carTrackerPair in _agentsAndTrackers)
            {
                carTrackerPair.Deconstruct(out AgentTracker tracker, out CarAgent carAgent);

                carAgent.transform.position = TrackManager.Instance.CurrentTrack.StartTransform.position;
                carAgent.transform.rotation = TrackManager.Instance.CurrentTrack.StartTransform.rotation;

                carAgent.ResetAgent();
            }
        }
    }
}

[Serializable]
public class TrainingParameters
{
    [SerializeField] private int _seed;
    [SerializeField] private int[] _neuronCounts = { 4, 24, 24, 4 };
    [SerializeField] private int _populationCount = 60;
    [SerializeField] private AnimationCurve _survivalChanceCurve;
    [SerializeField] private float _crossoverRate = 0.6f;
    [SerializeField] private float _mutationProbability = 0.01f;
    [SerializeField] private float _mutationRange = 5f;

    public int Seed => _seed;

    public int[] NeuronCounts => _neuronCounts;

    public int PopulationCount => _populationCount;

    public AnimationCurve SurvivalChanceCurve => _survivalChanceCurve;

    public float CrossoverRate => _crossoverRate;

    public float MutationProbability => _mutationProbability;

    public float MutationRange => _mutationRange;
}

public class AgentTracker
{
    public Perceptron perceptron;
    public float fitness;

    public AgentTracker(Perceptron perceptron)
    {
        this.perceptron = perceptron;
    }

    public override string ToString()
    {
        return $"Per:({perceptron}) | (Fit:{fitness})";
    }
}

public class AgentCollection
{
    private TrainingParameters _parameters;
    private List<AgentTracker> _agentTrackers;

    private Random _random;

    public List<AgentTracker> AgentTrackers => _agentTrackers;

    public Random Random => _random;

    public AgentCollection(TrainingParameters parameters)
    {
        _random = new Random(parameters.Seed);

        _parameters = parameters;
        _agentTrackers = new List<AgentTracker>();

        for (int i = 0; i < parameters.PopulationCount; i++)
        {
            Perceptron perceptron = new Perceptron(parameters.NeuronCounts, _random.Next());
            AgentTracker agentTracker = new AgentTracker(perceptron);
            _agentTrackers.Add(agentTracker);
        }
    }

    public void ApplySurvivalCurve()
    {
        // Order agents by worst to best, worst first, best last
        _agentTrackers = _agentTrackers.OrderBy(tracker => tracker.fitness).ToList();

        // Higher rank is better
        for (int rank = 0; rank < _agentTrackers.Count; rank++)
        {
            float rankNormalised = rank / (float)(_parameters.PopulationCount - 1);
            float survivalChance = _parameters.SurvivalChanceCurve.Evaluate(rankNormalised);
            float survivalValue = _random.NextFloat(0f, 1f);

            if (survivalValue > survivalChance)
            {
                _agentTrackers[rank].perceptron = null;
            }
        }
    }

    public void Repopulate()
    {
        List<AgentTracker> survivingTrackers = _agentTrackers.Where(tracker => tracker.perceptron != null).OrderByDescending(tracker => tracker.fitness).ToList();
        List<AgentTracker> emptyTrackers = _agentTrackers.Where(tracker => tracker.perceptron == null).ToList();

        for (int i = 0; i < emptyTrackers.Count; i += 2)
        {
            AgentTracker trackerOne = emptyTrackers[i];
            AgentTracker trackerTwo = i < emptyTrackers.Count - 1 ? emptyTrackers[i + 1] : null;

            // Select two parents
            AgentTracker parentOne = SelectParent(survivingTrackers);
            AgentTracker parentTwo = SelectParent(survivingTrackers, parentOne);

            // Export genomes
            SerialPerceptron genomeOne = parentOne.perceptron.ExportPerceptron();
            SerialPerceptron genomeTwo = parentTwo.perceptron.ExportPerceptron();

            CrossoverGenomes(genomeOne, genomeTwo);

            MutateGenome(genomeOne);
            MutateGenome(genomeTwo);

            Perceptron childPerceptronOne = Perceptron.CreatePerceptron(genomeOne, _random.Next());
            trackerOne.perceptron = childPerceptronOne;
            trackerOne.fitness = 0f;

            if (trackerTwo != null)
            {
                Perceptron childPerceptronTwo = Perceptron.CreatePerceptron(genomeTwo, _random.Next());
                trackerTwo.perceptron = childPerceptronTwo;
                trackerTwo.fitness = 0f;
            }
        }
    }

    public AgentTracker SelectParent(List<AgentTracker> sourcePool, AgentTracker filter = null)
    {
        float fitnessSum = sourcePool.Sum(tracker => tracker.fitness);

        float parentFitnessValue = _random.NextFloat(0, fitnessSum);
        float runningTotal = 0;

        foreach (AgentTracker tracker in sourcePool)
        {
            if (runningTotal + tracker.fitness >= parentFitnessValue && tracker != filter)
            {
                return tracker;
            }

            runningTotal += tracker.fitness;
        }

        return sourcePool.Last();
    }

    private void CrossoverGenomes(SerialPerceptron genomeOne, SerialPerceptron genomeTwo)
    {
        // Swap the weight genes
        for (int n = 0; n < genomeOne.weights.Count; n++)
        {
            for (int w = 0; w < genomeOne.weights[n].Length; w++)
            {
                float swapChance = _random.NextFloat();

                if (swapChance <= _parameters.CrossoverRate)
                {
                    (genomeOne.weights[n][w], genomeTwo.weights[n][w]) = (genomeTwo.weights[n][w], genomeOne.weights[n][w]);
                }
            }
        }

        // Swap the bias genes
        for (int b = 0; b < genomeOne.biases.Count; b++)
        {
            float swapChance = _random.NextFloat();

            if (swapChance <= _parameters.CrossoverRate)
            {
                (genomeOne.biases[b], genomeTwo.biases[b]) = (genomeTwo.biases[b], genomeOne.biases[b]);
            }
        }
    }

    private void MutateGenome(SerialPerceptron genome)
    {
        // Mutate the weight genes
        for (int n = 0; n < genome.weights.Count; n++)
        {
            for (int w = 0; w < genome.weights[n].Length; w++)
            {
                float mutateChance = _random.NextFloat();

                if (mutateChance <= _parameters.MutationProbability)
                {
                    genome.weights[n][w] = _random.NextFloat(_parameters.MutationRange * -1f, _parameters.MutationRange);
                }
            }
        }

        // Mutate the bias genes
        for (int b = 0; b < genome.biases.Count; b++)
        {
            float mutateChance = _random.NextFloat();

            if (mutateChance <= _parameters.MutationProbability)
            {
                genome.biases[b] = _random.NextFloat(_parameters.MutationRange * -1f, _parameters.MutationRange);
            }
        }
    }
}