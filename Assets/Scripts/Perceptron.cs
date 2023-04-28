using System;
using System.Linq;

namespace NeuralNet
{
    [Serializable]
    public class SerialPerceptron
    {
        public int[] neuronsPerLayer;
        public float[][] weights;
        public float[] biases;

        public SerialPerceptron(int[] neuronsPerLayer, float[][] weights, float[] biases)
        {
            this.neuronsPerLayer = neuronsPerLayer;
            this.weights = weights;
            this.biases = biases;
        }
    }
    
    public class Perceptron
    {
        private Layer[] _layers;

        public Layer[] Layers => _layers;

        private int _seed;

        public Perceptron(int[] neuronsPerLayer, int seed = -1)
        {
            seed = seed < 0 ? seed : DateTime.Now.GetHashCode();
            Random random = new Random(seed);

            _layers = new Layer[neuronsPerLayer.Length];

            for (int i = 0; i < neuronsPerLayer.Length; i++)
            {
                Layer previousLayer = i == 0 ? null : _layers[i - 1];
                Layer layer = new Layer(neuronsPerLayer[i], previousLayer, random, 5);
                _layers[i] = layer;
            }
        }

        public void SetInputActivations(float[] activations)
        {
            for (int a = 0; a < activations.Length; a++)
                _layers[0].Neurons[a].Activation = activations[a];
        }

        public float[] GetOutputActivations()
        {
            Layer lastLayer = _layers[_layers.Length - 1];
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

        public void ProcessInputActivations()
        {
            float[] inputActivations = _layers[0].GetActivations();

            for (int i = 1; i < _layers.Length; i++)
                inputActivations = _layers[i].ComputeActivations(inputActivations);
        }

        public SerialPerceptron ExportPerceptron()
        {
            int[] neuronsPerLayer = new int[_layers.Length];

            for (int i = 0; i < _layers.Length; i++)
                neuronsPerLayer[i] = _layers[i].Neurons.Length;

            int totalBiases = 0;
            for (int i = 1; i < _layers.Length; i++)
                totalBiases += neuronsPerLayer[i];

            float[][] weights = new float[totalBiases][];
            float[] biases = new float[totalBiases];

            for (int l = 1; l < _layers.Length; l++)
            {
                Layer layer = _layers[l];
                for (int n = 0; n < layer.Neurons.Length; n++)
                {
                    Neuron neuron = layer.Neurons[n];
                    weights[l - 1 + n] = neuron.Weights;
                    biases[l - 1 + n] = neuron.Bias;
                }
            }

            return new SerialPerceptron(neuronsPerLayer, weights, biases);
        }

        public static Perceptron CreatePerceptron(SerialPerceptron serialPerceptron)
        {
            Perceptron perceptron = new Perceptron(serialPerceptron.neuronsPerLayer);

            for (int l = 0; l < perceptron.Layers.Length; l++)
            {
                Layer layer = perceptron.Layers[l];
                for (int n = 0; n < layer.Neurons.Length; n++)
                {
                    Neuron neuron = layer.Neurons[n];
                    neuron.Weights = serialPerceptron.weights[l - 1 + n];
                    neuron.Bias = serialPerceptron.biases[l - 1 + n];
                }
            }

            return perceptron;
        }

        public override string ToString()
        {
            return $"Perceptron - Seed:{_seed} Neurons:{string.Join(", ", _layers.Select(layer => layer.Neurons.Length))}";
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
            return min + ((max - min) * random.NextFloat());
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