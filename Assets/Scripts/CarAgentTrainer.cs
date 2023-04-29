using System;
using System.Collections.Generic;
using System.Linq;
using NeuralNet;
using UnityEngine;
using Random = UnityEngine.Random;

public class CarAgentTrainer : MonoBehaviour
{
    [SerializeField] private TrainingParameters _parameters;

    [SerializeField] private GameObject _carAgentPrefab;

    [SerializeField] private Transform _spawnPoint;

    private Dictionary<AgentTracker, CarAgent> _agentsAndTrackers;

    private AgentCollection _agentCollection;
    private int _generationIndex;

    private void Start()
    {
        _agentCollection = new AgentCollection(_parameters);

        SpawnCarAgents();
    }

    private void SpawnCarAgents()
    {
        _agentsAndTrackers = new Dictionary<AgentTracker, CarAgent>();

        foreach (AgentTracker agentTracker in _agentCollection.AgentTrackers)
        {
            GameObject carAgent = Instantiate(_carAgentPrefab, _spawnPoint.position, Quaternion.identity, transform);
            _agentsAndTrackers.Add(agentTracker, carAgent.GetComponent<CarAgent>());
        }
    }

    private void Update()
    {
        float timeDelta = Time.deltaTime;

        int completedAgentsCounter = 0;

        foreach (KeyValuePair<AgentTracker, CarAgent> carTrackerPair in _agentsAndTrackers)
        {
            carTrackerPair.Deconstruct(out AgentTracker tracker, out CarAgent carAgent);

            if (carAgent.Crashed || carAgent.Finished)
            {
                completedAgentsCounter++;
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

            carAgent.UpdateWithTime(timeDelta);

            tracker.fitness = carAgent.TimeAlive;

            if (carAgent.Finished)
                tracker.fitness += 100;
        }

        // All agents have failed so time for a new generation
        if (completedAgentsCounter == _agentsAndTrackers.Count)
        {
            float maxBestFitness = _agentCollection.AgentTrackers.Max(tracker => tracker.fitness);
            UserInterface.Instance.UpdateText(_generationIndex++, maxBestFitness);

            _agentCollection.ApplySurvivalCurve();
            _agentCollection.Repopulate();

            // Reset all the cars
            foreach (KeyValuePair<AgentTracker, CarAgent> carTrackerPair in _agentsAndTrackers)
            {
                carTrackerPair.Deconstruct(out AgentTracker tracker, out CarAgent carAgent);

                carAgent.ResetAgent();

                carAgent.transform.position = _spawnPoint.position;
                carAgent.transform.rotation = Quaternion.identity;
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

    public List<AgentTracker> AgentTrackers => _agentTrackers;

    public AgentCollection(TrainingParameters parameters)
    {
        _parameters = parameters;
        _agentTrackers = new List<AgentTracker>();

        for (int i = 0; i < parameters.PopulationCount; i++)
        {
            Perceptron perceptron = new Perceptron(parameters.NeuronCounts, parameters.Seed + i);
            AgentTracker agentTracker = new AgentTracker(perceptron);
            _agentTrackers.Add(agentTracker);
        }
    }

    public void ApplySurvivalCurve()
    {
        _agentTrackers = _agentTrackers.OrderBy(tracker => tracker.fitness).ToList();

        for (int rank = 0; rank < _agentTrackers.Count; rank++)
        {
            float rankNormalised = rank / (float)(_parameters.PopulationCount - 1);
            float survivalChance = _parameters.SurvivalChanceCurve.Evaluate(rankNormalised);
            float survivalValue = Random.Range(0f, 1f);

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

            Perceptron childPerceptronOne = Perceptron.CreatePerceptron(genomeOne);
            trackerOne.perceptron = childPerceptronOne;

            if (trackerTwo != null)
            {
                Perceptron childPerceptronTwo = Perceptron.CreatePerceptron(genomeOne);
                trackerTwo.perceptron = childPerceptronTwo;
            }
        }
    }

    public AgentTracker SelectParent(List<AgentTracker> sourcePool, AgentTracker filter = null)
    {
        float fitnessSum = sourcePool.Sum(tracker => tracker.fitness);

        float parentFitnessValue = Random.Range(0, fitnessSum);
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
                float swapChance = Random.Range(0f, 1f);

                if (swapChance <= _parameters.CrossoverRate)
                {
                    float weightTemp = genomeOne.weights[n][w];
                    genomeOne.weights[n][w] = genomeTwo.weights[n][w];
                    genomeTwo.weights[n][w] = weightTemp;
                }
            }
        }

        // Swap the bias genes
        for (int b = 0; b < genomeOne.biases.Count; b++)
        {
            float swapChance = Random.Range(0f, 1f);

            if (swapChance <= _parameters.CrossoverRate)
            {
                float biasTemp = genomeOne.biases[b];
                genomeOne.biases[b] = genomeTwo.biases[b];
                genomeTwo.biases[b] = biasTemp;
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
                float mutateChance = Random.Range(0f, 1f);

                if (mutateChance <= _parameters.MutationProbability)
                {
                    genome.weights[n][w] = Random.Range(_parameters.MutationRange * -1f, _parameters.MutationRange);
                }
            }
        }

        // Mutate the bias genes
        for (int b = 0; b < genome.biases.Count; b++)
        {
            float mutateChance = Random.Range(0f, 1f);

            if (mutateChance <= _parameters.MutationProbability)
            {
                genome.biases[b] = Random.Range(_parameters.MutationRange * -1f, _parameters.MutationRange);
            }
        }
    }
}