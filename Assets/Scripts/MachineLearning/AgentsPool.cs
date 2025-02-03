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
            MultiLayerPerceptron perceptron = new MultiLayerPerceptron(_random.Next(), _parameters.LayerParams,
                _parameters.WeightInitialisationType);
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
            float rankNormalised = rank / (float)(_parameters.PopulationCount - 1);
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
        List<AgentEntity<TAgentBody>> survivingEntities = _agentEntities.Where(entity => entity.Perceptron != null)
            .OrderByDescending(tracker => tracker.Fitness).ToList();
        List<AgentEntity<TAgentBody>>
            emptyEntities = _agentEntities.Where(entity => entity.Perceptron == null).ToList();

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

            MultiLayerPerceptron childPerceptronOne = MultiLayerPerceptron.CreatePerceptron(genomeOne, _random.Next());
            entityOne.AssignBrain(childPerceptronOne);

            if (entityTwo != null)
            {
                MultiLayerPerceptron childPerceptronTwo =
                    MultiLayerPerceptron.CreatePerceptron(genomeTwo, _random.Next());
                entityTwo.AssignBrain(childPerceptronTwo);
            }
        }
    }

    public AgentEntity<TAgentBody> SelectParent(List<AgentEntity<TAgentBody>> sourcePool,
        AgentEntity<TAgentBody> filter = null)
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
        for (int n = 0; n < genomeOne.SerialNeurons.Length; n++)
        {
            SerialNeuron neuronOne = genomeOne.SerialNeurons[n];
            SerialNeuron neuronTwo = genomeTwo.SerialNeurons[n];

            // Swap the weight genes

            for (int w = 0; w < neuronOne.Weights.Length; w++)
            {
                float weightSwapChance = _random.NextFloat();

                if (weightSwapChance <= _parameters.CrossoverRate)
                {
                    (neuronOne.Weights[w], neuronTwo.Weights[w]) = (neuronTwo.Weights[w], neuronOne.Weights[w]);
                }
            }

            // Swap the bias genes

            float biasSwapChance = _random.NextFloat();

            if (biasSwapChance <= _parameters.CrossoverRate)
            {
                (neuronOne.Bias, neuronTwo.Bias) = (neuronTwo.Bias, neuronOne.Bias);
            }
        }
    }

    private void ApplyOffsetMutation(SerialPerceptron genome, float probability, float strength)
    {
        for (int n = 0; n < genome.SerialNeurons.Length; n++)
        {
            SerialNeuron neuron = genome.SerialNeurons[n];

            for (int w = 0; w < neuron.Weights.Length; w++)
            {
                float weightMutateChance = _random.NextFloat();

                if (weightMutateChance <= probability)
                {
                    neuron.Weights[w] += _random.NormalDistribution(0f, strength);
                }
            }

            float biasMutateChance = _random.NextFloat();

            if (biasMutateChance <= probability)
            {
                neuron.Bias += _random.NormalDistribution(0f, strength);
            }
        }
    }

    private void ApplyResetMutation(SerialPerceptron genome, float probability, float strength)
    {
        for (int n = 0; n < genome.SerialNeurons.Length; n++)
        {
            SerialNeuron neuron = genome.SerialNeurons[n];

            for (int w = 0; w < neuron.Weights.Length; w++)
            {
                float weightMutateChance = _random.NextFloat();

                if (weightMutateChance <= probability)
                {
                    neuron.Weights[w] = _random.NextFloat(-strength, strength);
                }
            }

            float biasMutateChance = _random.NextFloat();

            if (biasMutateChance <= probability)
            {
                neuron.Bias = _random.NextFloat(-strength, strength);
            }
        }
    }
}