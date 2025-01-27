using System;
using NeuralNet;
using UnityEngine;

[Serializable]
public class TrainingParameters
{
    [Header("General")]
    [SerializeField] private int _seed;

    [Header("MLP Params")]
    [SerializeField] private ActivationFunctions.ActivationFunctionType _activationFunctionType;
    [SerializeField] private int[] _neuronCounts = {8, 16, 8, 4};

    [Header("Genetic Params")]
    [SerializeField] private int _populationCount = 60;
    [SerializeField] private AnimationCurve _survivalChanceCurve;
    [SerializeField] private float _crossoverRate = 0.5f;
    [SerializeField] private float _mutationProbability = 0.01f;
    [SerializeField] private float _mutationRange = 5f;

    public int Seed => _seed;

    public ActivationFunctions.ActivationFunctionType ActivationFunctionType => _activationFunctionType;

    public int[] NeuronCounts => _neuronCounts;

    public int PopulationCount => _populationCount;

    public AnimationCurve SurvivalChanceCurve => _survivalChanceCurve;

    public float CrossoverRate => _crossoverRate;

    public float MutationProbability => _mutationProbability;

    public float MutationRange => _mutationRange;
}