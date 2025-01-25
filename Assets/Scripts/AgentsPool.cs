using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NeuralNet;

public class AgentsPool<TAgentBody> where TAgentBody : IAgentBody
{
    private TrainingParameters _parameters;
    private List<AgentEntity<TAgentBody>> _agentEntities;

    private Random _random;

    public List<AgentEntity<TAgentBody>> AgentEntities => _agentEntities;

    public Random Random => _random;

    public AgentsPool(TrainingParameters parameters)
    {
        _random = new Random(parameters.Seed);

        _parameters = parameters;
        _agentEntities = new List<AgentEntity<TAgentBody>>();

        for (int i = 0; i < parameters.PopulationCount; i++)
        {
            Perceptron perceptron = new Perceptron(parameters.NeuronCounts, _random.Next());
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

    public void Repopulate()
    {
        List<AgentEntity<TAgentBody>> survivingEntities = _agentEntities.Where(tracker => tracker.Perceptron != null).OrderByDescending(tracker => tracker.Fitness).ToList();
        List<AgentEntity<TAgentBody>> emptyEntities = _agentEntities.Where(tracker => tracker.Perceptron == null).ToList();

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

            MutateGenome(genomeOne);
            MutateGenome(genomeTwo);

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