using System;
using System.Collections.Generic;
using NeuralNet;
using UnityEngine;

[Serializable]
public class TrainingParameters
{
    [Serializable]
    public class GenerationBasedMutationParameter
    {
        [Serializable]
        public class Entry
        {
            public int GenerationBoundary;
            [Range(0f, 1f)] public float Probability;
            public float Strength;
        }

        [SerializeField] [Range(0f, 1f)] private float _globalMutationChance;
        [SerializeField] private Entry[] _entries;

        public float GlobalMutationChance => _globalMutationChance;

        public float GetProbability(int genIndex)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (genIndex <= _entries[i].GenerationBoundary)
                {
                    return _entries[i].Probability;
                }
            }

            return _entries[^1].Probability;
        }

        public float GetStrength(int genIndex)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (genIndex <= _entries[i].GenerationBoundary)
                {
                    return _entries[i].Strength;
                }
            }

            return _entries[^1].Strength;
        }
    }

    [Header("General")]
    [SerializeField] private int _seed;

    [Header("MLP Params")]
    [SerializeField] private WeightInitialisationType _weightInitialisationType;
    [SerializeField] private LayerParams[] _layerParams;

    [Header("Genetic Params")]
    [SerializeField] private int _populationCount = 150;
    [SerializeField] private AnimationCurve _survivalChanceCurve;

    [Tooltip("Determines the percentage chance that a given weight or bias (gene) will be swapped when producing off spring from two parent genomes")]
    [SerializeField] private float _crossoverRate = 0.6f;

    [Tooltip("Determines the percentage chance that a given weight or bias (gene) will be offset by a normal distribution value")]
    [SerializeField] private GenerationBasedMutationParameter _offsetMutationParams = new GenerationBasedMutationParameter();

    [Tooltip("Determines the percentage chance that a given weight or bias (gene) will be replaced by a new random value")]
    [SerializeField] private GenerationBasedMutationParameter _resetMutationParams = new GenerationBasedMutationParameter();

    public int Seed => _seed;

    public WeightInitialisationType WeightInitialisationType => _weightInitialisationType;

    public LayerParams[] LayerParams => _layerParams;

    public int PopulationCount => _populationCount;

    public AnimationCurve SurvivalChanceCurve => _survivalChanceCurve;

    public float CrossoverRate => _crossoverRate;

    public GenerationBasedMutationParameter OffsetMutationParams => _offsetMutationParams;

    public GenerationBasedMutationParameter ResetMutationParams => _resetMutationParams;
}