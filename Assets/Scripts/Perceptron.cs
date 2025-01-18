using System;
using System.Collections.Generic;
using System.Linq;
using static Unity.Burst.Intrinsics.Arm;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace NeuralNet
{
    [Serializable]
    public class SerialPerceptron
    {
        public List<int> neuronsPerLayer;
        public List<float[]> weights;
        public List<float> biases;

        public SerialPerceptron(List<int> neuronsPerLayer, List<float[]> weights, List<float> biases)
        {
            this.neuronsPerLayer = neuronsPerLayer;
            this.weights = weights;
            this.biases = biases;
        }

        public override string ToString()
        {
            return $"Hashcode:{GetHashCode()}";
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            for (int i = 0; i < neuronsPerLayer.Count; i++)
                hashCode.Add(neuronsPerLayer[i]);

            for (int n = 0; n < weights.Count; n++)
            {
                for (int w = 0; w < weights[n].Length; w++)
                {
                    hashCode.Add(weights[n][w]);
                }

                hashCode.Add(biases[n]);
            }


            return hashCode.ToHashCode();
        }
    }

    public class Perceptron
    {
        private Layer[] _layers;

        public Layer[] Layers => _layers;

        private int _seed;

        public int Seed => _seed;

        public Perceptron(int[] neuronsPerLayer, int seed)
        {
            _seed = seed == -1 ? DateTime.Now.GetHashCode() : seed;
            Random random = new Random(seed);

            _layers = new Layer[neuronsPerLayer.Length];

            for (int i = 0; i < neuronsPerLayer.Length; i++)
            {
                Layer previousLayer = i == 0 ? null : _layers[i - 1];
                Layer layer = new Layer(neuronsPerLayer[i], previousLayer, random, 5);
                _layers[i] = layer;
            }
        }

        public float[] ProcessInputsToOutputs(float[] activations)
        {
            SetInputActivations(activations);
            ProcessInputActivations();
            return GetOutputActivations();
        }

        public void SetInputActivations(float[] activations)
        {
            for (int a = 0; a < activations.Length; a++)
                _layers[0].Neurons[a].Activation = activations[a];
        }

        public void ProcessInputActivations()
        {
            float[] inputActivations = _layers[0].GetActivations();

            for (int i = 1; i < _layers.Length; i++)
                inputActivations = _layers[i].ComputeActivations(inputActivations);
        }

        public float[] GetOutputActivations()
        {
            Layer lastLayer = _layers[^1];
            float[] outputActivations = new float[lastLayer.Neurons.Length];

            for (int i = 0; i < lastLayer.Neurons.Length; i++)
            {
                outputActivations[i] = lastLayer.Neurons[i].Activation;
            }

            return outputActivations;
        }

        public int GetMaxOutputActivationIndex(out float maxActivation)
        {
            float[] outputActivations = GetOutputActivations();

            int maxIndex = 0;
            maxActivation = float.MinValue;

            for (int i = 0; i < outputActivations.Length; i++)
            {
                if (maxActivation > outputActivations[i])
                    continue;

                maxIndex = i;
                maxActivation = outputActivations[i];
            }

            return maxIndex;
        }

        public SerialPerceptron ExportPerceptron()
        {
            List<int> neuronsPerLayer = new List<int>();

            for (int i = 0; i < _layers.Length; i++)
                neuronsPerLayer.Add(_layers[i].Neurons.Length);

            List<float[]> weights = new List<float[]>();
            List<float> biases = new List<float>();

            for (int l = 1; l < _layers.Length; l++)
            {
                Layer layer = _layers[l];
                for (int n = 0; n < layer.Neurons.Length; n++)
                {
                    Neuron neuron = layer.Neurons[n];

                    weights.Add((float[])neuron.Weights.Clone());
                    biases.Add(neuron.Bias);
                }
            }

            return new SerialPerceptron(neuronsPerLayer, weights, biases);
        }

        public static Perceptron CreatePerceptron(SerialPerceptron serialPerceptron, int seed)
        {
            Perceptron perceptron = new Perceptron(serialPerceptron.neuronsPerLayer.ToArray(), seed);

            int neuronCounter = 0;
            for (int l = 1; l < perceptron.Layers.Length; l++)
            {
                Layer layer = perceptron.Layers[l];
                for (int n = 0; n < layer.Neurons.Length; n++)
                {
                    Neuron neuron = layer.Neurons[n];
                    neuron.Weights = (float[])serialPerceptron.weights[neuronCounter].Clone();
                    neuron.Bias = serialPerceptron.biases[neuronCounter];
                    neuronCounter++;
                }
            }

            return perceptron;
        }

        public override string ToString()
        {
            return $"Seed:{_seed} Hashcode:{GetHashCode()}";
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            for (int i = 0; i < _layers.Length; i++)
                hashCode.Add(_layers[i].Neurons.Length);

            for (int l = 1; l < _layers.Length; l++)
            {
                Layer layer = _layers[l];
                for (int n = 0; n < layer.Neurons.Length; n++)
                {
                    Neuron neuron = layer.Neurons[n];

                    for (int w = 0; w < neuron.Weights.Length; w++)
                        hashCode.Add(neuron.Weights[w]);

                    hashCode.Add(neuron.Bias);
                }
            }

            return hashCode.ToHashCode();
        }
    }

    public class Layer
    {
        private Neuron[] _neurons;

        public Neuron[] Neurons => _neurons;

        public Layer(int neuronCount, Layer previousLayer, Random random, float randomRange)
        {
            _neurons = new Neuron[neuronCount];
            for (int i = 0; i < neuronCount; i++)
            {
                int inputNeuronCount = previousLayer == null ? 0 : previousLayer.Neurons.Length;
                Neuron neuron = new Neuron(inputNeuronCount, random, randomRange);
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

    public class Neuron
    {
        private float[] _weights;
        private float _bias;
        private float _activation;

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

        public Neuron(int inputNeuronCount, Random random, float randomRange)
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
        }

        public float ComputeActivation(float[] inputActivations)
        {
            _activation = 0;

            for (int i = 0; i < _weights.Length; i++)
                _activation += inputActivations[i] * _weights[i];

            _activation += _bias;
            _activation = MathFunctions.Sigmoid(_activation);

            return _activation;
        }
    }

    public static class RandomExtensions
    {
        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }

        public static float NextFloat(this Random random, float min, float max)
        {
            return min + ((max - min) * ((float)random.NextDouble()));
        }
    }

    public static class MathFunctions
    {
        public static float Sigmoid(float input)
        {
            return (1f / (1f + (float)Math.Exp(-input)));
        }

        public static float ReLU(float input)
        {
            return input <= 0f ? 0f : input;
        }
    }
}
