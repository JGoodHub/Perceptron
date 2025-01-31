using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GoodHub.Core.Runtime.Utils;
using NeuralNet;

public class AgentsPool<TAgentBody> where TAgentBody : IAgentBody
{
    private TrainingParameters _parameters;
    private List<AgentEntity<TAgentBody>> _agentEntities;

    private Random _random;

    public List<AgentEntity<TAgentBody>> AgentEntities => _agentEntities;

    public AgentsPool(TrainingParameters parameters)
    {
        _parameters = parameters;

        _random = new Random(_parameters.Seed);

        _agentEntities = new List<AgentEntity<TAgentBody>>();

        for (int i = 0; i < _parameters.PopulationCount; i++)
        {
            Perceptron perceptron = new Perceptron(_random.Next(), _parameters.LayerParams, _parameters.WeightInitialisationType);
            AgentEntity<TAgentBody> agentTracker = new AgentEntity<TAgentBody>(perceptron);
            _agentEntities.Add(agentTracker);
        }
    }

    public void ApplySurvivalCurve()
    {
        // Order agents by worst to best, worst first, best last
        _agentEntities = _agentEntities.OrderBy(entity => entity.Fitness).ToList();

        // Higher rank is better
        for (int rank = 0; rank < _agentEntities.Count; rank++)
        {
            float rankNormalised = rank / (float) (_parameters.PopulationCount - 1);
            float survivalChance = _parameters.SurvivalChanceCurve.Evaluate(rankNormalised);
            float survivalValue = _random.NextFloat(0f, 1f);

            if (survivalValue > survivalChance)
            {
                _agentEntities[rank].AssignBrain(null);
            }
        }
    }

    public void Repopulate(int generationIndex)
    {
        List<AgentEntity<TAgentBody>> survivingEntities = _agentEntities.Where(entity => entity.Perceptron != null).OrderByDescending(tracker => tracker.Fitness).ToList();
        List<AgentEntity<TAgentBody>> emptyEntities = _agentEntities.Where(entity => entity.Perceptron == null).ToList();

        for (int i = 0; i < emptyEntities.Count; i += 2)
        {
            AgentEntity<TAgentBody> entityOne = emptyEntities[i];
            AgentEntity<TAgentBody> entityTwo = i < emptyEntities.Count - 1 ? emptyEntities[i + 1] : null;

            // Select two parents
            AgentEntity<TAgentBody> parentOne = SelectParent(survivingEntities);
            AgentEntity<TAgentBody> parentTwo = SelectParent(survivingEntities, parentOne);

            // Export genomes
            SerialPerceptron genomeOne = parentOne.Perceptron.ExportPerceptron();
            SerialPerceptron genomeTwo = parentTwo.Perceptron.ExportPerceptron();

            CrossoverGenomes(genomeOne, genomeTwo);

            if (_random.NextFloat() <= _parameters.OffsetMutationParams.GlobalMutationChance)
            {
                float probability = _parameters.OffsetMutationParams.GetProbability(generationIndex);
                float strength = _parameters.OffsetMutationParams.GetStrength(generationIndex);

                ApplyOffsetMutation(genomeOne, probability, strength);
                ApplyOffsetMutation(genomeTwo, probability, strength);
            }

            if (_random.NextFloat() <= _parameters.ResetMutationParams.GlobalMutationChance)
            {
                float probability = _parameters.ResetMutationParams.GetProbability(generationIndex);
                float strength = _parameters.ResetMutationParams.GetStrength(generationIndex);

                ApplyResetMutation(genomeOne, probability, strength);
                ApplyResetMutation(genomeTwo, probability, strength);
            }

            Perceptron childPerceptronOne = Perceptron.CreatePerceptron(genomeOne, _random.Next());
            entityOne.AssignBrain(childPerceptronOne);

            if (entityTwo != null)
            {
                Perceptron childPerceptronTwo = Perceptron.CreatePerceptron(genomeTwo, _random.Next());
                entityTwo.AssignBrain(childPerceptronTwo);
            }
        }
    }

    public AgentEntity<TAgentBody> SelectParent(List<AgentEntity<TAgentBody>> sourcePool, AgentEntity<TAgentBody> filter = null)
    {
        float fitnessSum = sourcePool.Sum(entity => entity.Fitness);

        float parentFitnessValue = _random.NextFloat(0, fitnessSum);
        float runningTotal = 0;

        foreach (AgentEntity<TAgentBody> entity in sourcePool)
        {
            if (runningTotal + entity.Fitness >= parentFitnessValue && entity != filter)
            {
                return entity;
            }

            runningTotal += entity.Fitness;
        }

        return sourcePool.Last();
    }

    private void CrossoverGenomes(SerialPerceptron genomeOne, SerialPerceptron genomeTwo)
    {
        // Swap the weight genes
        for (int n = 0; n < genomeOne.Weights.Length; n++)
        {
            for (int w = 0; w < genomeOne.Weights[n].Length; w++)
            {
                float swapChance = _random.NextFloat();

                if (swapChance <= _parameters.CrossoverRate)
                {
                    (genomeOne.Weights[n][w], genomeTwo.Weights[n][w]) = (genomeTwo.Weights[n][w], genomeOne.Weights[n][w]);
                }
            }
        }

        // Swap the bias genes
        for (int b = 0; b < genomeOne.Biases.Length; b++)
        {
            float swapChance = _random.NextFloat();

            if (swapChance <= _parameters.CrossoverRate)
            {
                (genomeOne.Biases[b], genomeTwo.Biases[b]) = (genomeTwo.Biases[b], genomeOne.Biases[b]);
            }
        }
    }

    private void ApplyOffsetMutation(SerialPerceptron genome, float probability, float strength)
    {
        // Mutate the weight genes
        for (int n = 0; n < genome.Weights.Length; n++)
        {
            for (int w = 0; w < genome.Weights[n].Length; w++)
            {
                float mutateChance = _random.NextFloat();

                if (mutateChance <= probability)
                {
                    genome.Weights[n][w] += _random.NormalDistribution(0f, strength);
                }
            }
        }

        // Mutate the bias genes
        for (int b = 0; b < genome.Biases.Length; b++)
        {
            float mutateChance = _random.NextFloat();

            if (mutateChance <= probability)
            {
                genome.Biases[b] += _random.NormalDistribution(0f, strength);
            }
        }
    }

    private void ApplyResetMutation(SerialPerceptron genome, float probability, float strength)
    {
        // Mutate the weight genes
        for (int n = 0; n < genome.Weights.Length; n++)
        {
            for (int w = 0; w < genome.Weights[n].Length; w++)
            {
                float mutateChance = _random.NextFloat();

                if (mutateChance <= probability)
                {
                    genome.Weights[n][w] = _random.NextFloat(-strength, strength);
                }
            }
        }

        // Mutate the bias genes
        for (int b = 0; b < genome.Biases.Length; b++)
        {
            float mutateChance = _random.NextFloat();

            if (mutateChance <= probability)
            {
                genome.Biases[b] = _random.NextFloat(-strength, strength);
            }
        }
    }
}