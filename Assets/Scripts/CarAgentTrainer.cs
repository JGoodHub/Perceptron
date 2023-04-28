using System;
using System.Collections.Generic;
using System.Linq;
using NeuralNet;
using UnityEngine;
using Random = System.Random;

public class CarAgentTrainer : MonoBehaviour
{
    [SerializeField] private TrainingParameters _parameters;

    [SerializeField] private GameObject _carAgentPrefab;

    [SerializeField] private Transform _spawnPoint;
    
    private Dictionary<AgentTracker, CarAgent> _carAgents;
    
    private AgentCollection _agentCollection;

    private void Start()
    {
        _agentCollection = new AgentCollection(_parameters);
    }

    private void SpawnCarAgents()
    {
        _carAgents = new Dictionary<AgentTracker, CarAgent>();
        
        foreach (AgentTracker agentTracker in _agentCollection.AgentTrackers)
        {
            GameObject carAgent = Instantiate(_carAgentPrefab, _spawnPoint.position, Quaternion.identity, transform);
            _carAgents.Add(agentTracker, carAgent.GetComponent<CarAgent>());
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
        return $"P: {perceptron} - F: {fitness}";
    }
}


public class AgentCollection
{
    private List<AgentTracker> _agentTrackers;

    public List<AgentTracker> AgentTrackers => _agentTrackers;

    public AgentCollection(TrainingParameters parameters)
    {
        _agentTrackers = new List<AgentTracker>();

        for (int i = 0; i < parameters.PopulationCount; i++)
        {
            Perceptron perceptron = new Perceptron(parameters.NeuronCounts, parameters.Seed + i);
            AgentTracker agentTracker = new AgentTracker(perceptron);
            _agentTrackers.Add(agentTracker);
        }
    }
    
    
    
}