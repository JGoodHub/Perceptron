using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class SpotDetectorController : MonoBehaviour
{
    [SerializeField] private int _seed;
    [Space] 
    [SerializeField] private int _hiddenLayerNeurons = 20;
    [SerializeField] private int _populationCount = 100;
    [SerializeField] private float _mutationRange = 0.5f;
    [SerializeField] private float _survivalChance = 0.5f;
    [SerializeField] private int _generationCount = 1000;
    [SerializeField] private int _testCount = 1000;
    [SerializeField, Range(0.02f, 1f)] private float _pauseDuration;

    private List<Perceptron> _perceptronPool;
    private Random _random;

    private int _correctGuesses;
    private int _incorrectGuesses;

    private int _guessesTotal;

    private Vector2 _spotPosition;
    private int _spotLocation;

    private void Start()
    {
        List<int> neuronCounts = new List<int>
        {
            2, _hiddenLayerNeurons, 4
        };

        _random = new Random(_seed);
        _perceptronPool = new List<Perceptron>();

        for (int i = 0; i < _populationCount; i++)
        {
            Random popRandom = new Random(_seed + 1);
            Perceptron perceptron = new Perceptron(neuronCounts, popRandom);
            _perceptronPool.Add(perceptron);
        }

        StartCoroutine(GenerationStep());
    }

    private IEnumerator GenerationStep()
    {
        yield return null;
        
        for (int genIndex = 0; genIndex < _generationCount; genIndex++)
        {
            // Generation stats

            float bestGenerationFitness = 0;
            
            // Prepare the tests for this generation

            List<List<float>> generationTestInputs = new List<List<float>>();
            List<int> generationTestAnswers = new List<int>();

            for (int testIndex = 0; testIndex < _testCount; testIndex++)
            {
                List<float> spotInput = new List<float> { Perceptron.RandomFloat(_random, 0f, 1f), Perceptron.RandomFloat(_random, 0f, 1f) };

                generationTestInputs.Add(spotInput);

                int spotAnswer = 0;
                if (spotInput[0] < 0.5f)
                    spotAnswer = spotInput[1] < 0.5f ? 2 : 0;
                else
                    spotAnswer = spotInput[1] < 0.5f ? 3 : 1;

                generationTestAnswers.Add(spotAnswer);
            }

            // Test the current generation of perceptrons

            List<KeyValuePair<Perceptron, float>> popFitnessMap = new List<KeyValuePair<Perceptron, float>>();

            foreach (Perceptron perceptron in _perceptronPool)
            {
                int correctGuesses = 0;

                for (int t = 0; t < generationTestInputs.Count; t++)
                {
                    perceptron.SetInputActivations(generationTestInputs[t]);

                    perceptron.ProcessInputActivations();

                    int outputIndex = perceptron.GetMaxOutputActivationIndex(out _);

                    if (outputIndex == generationTestAnswers[t])
                        correctGuesses++;
                }

                // Evaluate the fitness of this perceptron

                float fitness = correctGuesses / (float)_testCount;
                KeyValuePair<Perceptron, float> keyValuePair = new KeyValuePair<Perceptron, float>(perceptron, fitness);
                popFitnessMap.Add(keyValuePair);

                if (fitness > bestGenerationFitness)
                    bestGenerationFitness = fitness;
            }

            // Organised the pops by fitness and decimate

            popFitnessMap = popFitnessMap.OrderByDescending(popFitness => popFitness.Value).ToList();

            int decimationStartIndex = Mathf.RoundToInt(popFitnessMap.Count / _survivalChance);
            for (int p = decimationStartIndex; p < popFitnessMap.Count; p++)
            {
                _perceptronPool.RemoveAt(p);
            }

            // Create new pops from the survivors

            while (_perceptronPool.Count < _populationCount)
            {
                // Take a random pop and clone it
                
                Perceptron randomPop = _perceptronPool[_random.Next(0, decimationStartIndex)];
                Perceptron popClone = randomPop.Clone();
                
                // Give it some mutations and add it to the pool
                
                popClone.ApplyRandomWeightAndBiasMutations(_mutationRange);
                _perceptronPool.Add(popClone);
            }
            
            Debug.Log($"Generation {genIndex} done, best fitness was {bestGenerationFitness}");

            yield return new WaitForSeconds(_pauseDuration);
        }
    }
}
