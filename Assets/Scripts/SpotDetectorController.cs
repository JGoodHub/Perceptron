// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using MLP;
// using NeuralNet;
// using UnityEngine;
// using UnityEngine.Serialization;
// using Random = System.Random;
//
// public class SpotDetectorController : MonoBehaviour
// {
//     public class FitnessTracker
//     {
//         public Perceptron Perceptron;
//         public float Fitness;
//
//         public override string ToString()
//         {
//             return $"P:{Perceptron.GetHashCode()} - F: {Fitness}";
//         }
//     }
//
//     
//
//     [SerializeField] private int _seed;
//     [SerializeField] private int[] _neuronCounts = { 2, 14, 14, 4 };
//     [SerializeField] private int _populationCount = 100;
//     [SerializeField] private float _mutationChance = 0.5f;
//     [SerializeField] private float _mutationRange = 0.5f;
//     [SerializeField] private float _survivalChance = 0.5f;
//     [SerializeField] private float _newSpeciesThreshold = 0.01f;
//     [SerializeField] private int _generationCount = 1000;
//     [SerializeField] private int _testCount = 1000;
//
//     private List<FitnessTracker> _tracker;
//     private Random _random;
//
//     private int _correctGuesses;
//     private int _incorrectGuesses;
//
//     private int _guessesTotal;
//
//     private Vector2 _spotPosition;
//     private int _spotLocation;
//
//     private void Start()
//     {
//         _random = new Random(_seed == -1 ? DateTime.Now.GetHashCode() : _seed);
//         _tracker = new List<FitnessTracker>();
//
//         for (int i = 0; i < _populationCount; i++)
//         {
//             Random popRandom = new Random(_seed + i);
//             Perceptron perceptron = new Perceptron(_neuronCounts, popRandom);
//
//             _tracker.Add(new FitnessTracker
//             {
//                 Perceptron = perceptron,
//                 Fitness = 0
//             });
//         }
//
//         StartCoroutine(GenerationStep());
//     }
//
//     private IEnumerator GenerationStep()
//     {
//         yield return null;
//         
//         // Prepare the tests for this generation
//
//         List<float[]> fitnessTestInputs = new List<float[]>();
//         List<int> generationTestAnswers = new List<int>();
//
//         for (int testIndex = 0; testIndex < _testCount; testIndex++)
//         {
//             float x = _random.NextFloat(0f, 1f);
//             float y = _random.NextFloat( 0f, 1f); 
//             float[] spotInput = { x, y };
//
//             fitnessTestInputs.Add(spotInput);
//
//             int spotAnswer;
//             if (x < 0.5f)
//                 spotAnswer = y < 0.5f ? 2 : 0;
//             else
//                 spotAnswer = y < 0.5f ? 3 : 1;
//
//             generationTestAnswers.Add(spotAnswer);
//         }
//
//         for (int genIndex = 0; genIndex < _generationCount; genIndex++)
//         {
//             // Generation stats
//
//             float bestGenerationFitness = 0;
//
//             // Test the current generation of perceptrons in parallel
//
//             Parallel.ForEach(_tracker, tracker =>
//             {
//                 int correctGuesses = 0;
//
//                 for (int i = 0; i < fitnessTestInputs.Count; i++)
//                 {
//                     tracker.Perceptron.SetInputActivations(fitnessTestInputs[i]);
//
//                     tracker.Perceptron.ProcessInputActivations();
//
//                     int outputIndex = tracker.Perceptron.GetMaxOutputActivationIndex(out _);
//
//                     if (outputIndex == generationTestAnswers[i])
//                         correctGuesses++;
//                 }
//
//                 // Evaluate the fitness of this perceptron
//
//                 tracker.Fitness = correctGuesses / (float)_testCount;
//
//                 if (tracker.Fitness > bestGenerationFitness)
//                     bestGenerationFitness = tracker.Fitness;
//             });
//
//             // Organised the pops by fitness and decimate
//
//             _tracker = _tracker.OrderByDescending(tracker => tracker.Fitness).ToList();
//
//             int decimationStartIndex = Mathf.RoundToInt(_tracker.Count * _survivalChance);
//             for (int p = decimationStartIndex; p < _tracker.Count; p++)
//             {
//                 _tracker.RemoveAt(decimationStartIndex);
//             }
//
//             // Create new pops from the survivors
//
//             while (_tracker.Count < _populationCount)
//             {
//                 float newSpeciesChance = _random.NextFloat();
//
//                 if (_newSpeciesThreshold >= newSpeciesChance)
//                 {
//                     // Spawn an entirely random pop
//                     
//                     Debug.LogError("New Species Spawned");
//
//                     Random popRandom = new Random(_random.Next(0, 9999));
//                     Perceptron perceptron = new Perceptron(_neuronCounts, popRandom);
//
//                     _tracker.Add(new FitnessTracker
//                     {
//                         Perceptron = perceptron,
//                         Fitness = 0
//                     });
//                 }
//                 else
//                 {
//                     // Take a random pop and clone it
//
//                     Perceptron randomPop = _tracker[_random.Next(0, decimationStartIndex)].Perceptron;
//                     Perceptron popClone = randomPop.Clone();
//
//                     // Give it some mutations
//
//                     popClone.MutateWeightsAndBiases(_mutationChance, _mutationRange);
//
//                     _tracker.Add(new FitnessTracker
//                     {
//                         Perceptron = popClone,
//                         Fitness = 0
//                     });
//                 }
//             }
//
//             if (genIndex % 10 == 0)
//             {
//                 Debug.Log($"Generation {genIndex} done, best fitness was {bestGenerationFitness}");
//                 yield return null;
//             }
//         }
//     }
// }