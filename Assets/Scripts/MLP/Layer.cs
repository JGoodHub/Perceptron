using System;
using UnityEngine;
using Random = System.Random;

namespace NeuralNet
{
    [Serializable]
    public class LayerParams
    {
        [SerializeField] private int _neuronCount;

        [SerializeField] private ActivationFunctionType _activationFunctionType;

        public int NeuronCount => _neuronCount;

        public ActivationFunctionType ActivationFunctionType => _activationFunctionType;
    }

    /// <summary>
    /// Represents a single layer in a multilayer perceptron (MLP). <br/>
    /// A layer is a fundamental building block of a neural network that consists of a collection of neurons.
    /// Each layer processes incoming activations and transforms it into meaningful outputs, which are passed to subsequent layers.
    /// </summary>
    public class Layer
    {
        private readonly Neuron[] _neurons;

        public Neuron[] Neurons => _neurons;

        public Layer(Random random, LayerParams layerParams,
            WeightInitialisationType weightInitializationType,
            int fanInCount, int fanOutCount)
        {
            _neurons = new Neuron[layerParams.NeuronCount];
            for (int i = 0; i < _neurons.Length; i++)
            {
                float[] weights = weightInitializationType switch
                {
                    WeightInitialisationType.None => Array.Empty<float>(),
                    WeightInitialisationType.Manual => Array.Empty<float>(),
                    WeightInitialisationType.XavierInitialisation =>
                        WeightInitialisations.XavierInitialisation(random, fanInCount, fanOutCount),
                    WeightInitialisationType.HeInitialization =>
                        WeightInitialisations.HeInitialization(random, fanInCount),
                };

                Neuron neuron = new Neuron(weights, 0f, layerParams.ActivationFunctionType);
                _neurons[i] = neuron;
            }
        }

        /// <summary>
        /// Get the current activations of all Neurons in this layer.
        /// </summary>
        /// <returns>An array of activations of all Neurons in this layer</returns>
        public float[] GetActivations()
        {
            float[] activations = new float[_neurons.Length];

            for (int i = 0; i < _neurons.Length; i++)
                activations[i] = _neurons[i].Activation;

            return activations;
        }

        /// <summary>
        /// Compute the activations for the Neurons in this layer of the MLP.
        /// </summary>
        /// <param name="precedingLayerActivations">Activations outputted by the preceding layer of the network</param>
        /// <returns>An array of activations of all Neurons in this layer</returns>
        public float[] ComputeActivations(float[] precedingLayerActivations)
        {
            float[] activations = new float[_neurons.Length];

            for (int i = 0; i < _neurons.Length; i++)
                activations[i] = _neurons[i].ComputeActivation(precedingLayerActivations);

            return activations;
        }
    }
}