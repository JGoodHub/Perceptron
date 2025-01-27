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

        public Neuron(int inputNeuronCount, Random random, float randomRange,
            ActivationFunctions.ActivationFunctionType activationFunctionType)
        {
            if (inputNeuronCount == 0)
            {
                _weights = new float[0];
                _bias = 0;
                return;
            }

            _weights = new float[inputNeuronCount];

            _bias = random.NextFloat(-randomRange, randomRange);
            _weights = new float[inputNeuronCount];

            for (int i = 0; i < _weights.Length; i++)
                _weights[i] = random.NextFloat(-randomRange, randomRange);

            _activationFunction = ActivationFunctions.ForType(activationFunctionType);
        }

        public float ComputeActivation(float[] inputActivations)
        {
            _activation = 0;

            for (int i = 0; i < _weights.Length; i++)
                _activation += inputActivations[i] * _weights[i];

            _activation += _bias;
            _activation = _activationFunction(_activation);

            return _activation;
        }
    }
}