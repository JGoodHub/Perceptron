using System;
using GoodHub.Core.Runtime.Utils;

namespace NeuralNet
{
    public class Neuron
    {
        private float[] _weights;
        private float _bias;
        private float _activation;

        private Func<float, float> _activationFunction;

        public float[] Weights
        {
            get => _weights;
            set
            {
                _weights = new float[value.Length];
                for (int w = 0; w < value.Length; w++)
                    _weights[w] = value[w];
            }
        }

        public float Bias
        {
            get => _bias;
            set => _bias = value;
        }

        public float Activation
        {
            get => _activation;
            set => _activation = value;
        }

        public Neuron(float[] weights, float bias)
        {
            _weights = new float[weights.Length];

            for (int i = 0; i < weights.Length; i++)
                _weights[i] = weights[i];

            _bias = bias;
        }

        public Neuron(float[] initialWeights, float initialBias, ActivationFunctionType activationFunctionType)
        {
            _weights = (float[]) initialWeights.Clone();
            _bias = initialBias;

            _activationFunction = ActivationFunctions.ForType(activationFunctionType);
        }

        /// <summary>
        /// Compute this neurons activation using the input activations from the
        /// previous layer plus this neurons weights for each activation.
        /// Offset by some bias and clamped with the given activation function.
        /// </summary>
        public float ComputeActivation(float[] inputActivations)
        {
            if (inputActivations.Length != _weights.Length)
            {
                throw new Exception("Length mismatch between " +
                                    $"{nameof(inputActivations)}({inputActivations.Length})" +
                                    $"and {nameof(_weights)}({_weights.Length}).");
            }

            _activation = 0;

            for (int i = 0; i < _weights.Length; i++)
                _activation += inputActivations[i] * _weights[i];

            _activation += _bias;
            _activation = _activationFunction(_activation);

            return _activation;
        }
    }
}