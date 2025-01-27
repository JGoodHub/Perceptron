using System;

namespace NeuralNet
{
    public class Layer
    {
        private Neuron[] _neurons;

        public Neuron[] Neurons => _neurons;

        public Layer(int neuronCount, Layer previousLayer, Random random, float randomRange,
            ActivationFunctions.ActivationFunctionType activationFunctionType)
        {
            _neurons = new Neuron[neuronCount];
            for (int i = 0; i < neuronCount; i++)
            {
                int inputNeuronCount = previousLayer == null ? 0 : previousLayer.Neurons.Length;
                Neuron neuron = new Neuron(inputNeuronCount, random, randomRange, activationFunctionType);
                _neurons[i] = neuron;
            }
        }

        public float[] GetActivations()
        {
            float[] activations = new float[_neurons.Length];

            for (int i = 0; i < _neurons.Length; i++)
                activations[i] = _neurons[i].Activation;

            return activations;
        }

        public float[] ComputeActivations(float[] inputActivations)
        {
            float[] activations = new float[_neurons.Length];

            for (int i = 0; i < _neurons.Length; i++)
                activations[i] = _neurons[i].ComputeActivation(inputActivations);

            return activations;
        }
    }
}