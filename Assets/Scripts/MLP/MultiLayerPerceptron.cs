using System;
using System.Collections.Generic;
using Random = System.Random;

namespace NeuralNet
{
    public class MultiLayerPerceptron
    {
        private readonly int _seed;

        private readonly LayerParams[] _layerParams;
        private readonly Layer[] _layers;

        private readonly WeightInitialisationType _weightInitializationType;
        private readonly ActivationFunctionType _activationFunctionType;

        public int Seed => _seed;

        public MultiLayerPerceptron(int seed, LayerParams[] layerParams,
            WeightInitialisationType weightInitializationType)
        {
            _seed = seed <= -1 ? DateTime.Now.GetHashCode() : seed;
            Random random = new Random(_seed);

            _layerParams = layerParams;
            _layers = new Layer[_layerParams.Length];

            _weightInitializationType = weightInitializationType;

            for (int i = 0; i < _layerParams.Length; i++)
            {
                WeightInitialisationType weightInitialisationType =
                    i == 0 ? WeightInitialisationType.None : _weightInitializationType;

                int fanInCount = i == 0 ? 0 : _layerParams[i - 1].NeuronCount;
                int fanOutCount = i == _layerParams.Length - 1 ? 0 : _layerParams[i + 1].NeuronCount;

                Layer layer = new Layer(random,
                    _layerParams[i],
                    weightInitialisationType,
                    fanInCount, fanOutCount
                );

                _layers[i] = layer;
            }
        }

        public float[] Process(float[] firstLayerActivations)
        {
            SetFirstLayerActivations(firstLayerActivations);
            ProcessLayers();
            return GetOutputActivations();
        }

        private void SetFirstLayerActivations(float[] activations)
        {
            for (int a = 0; a < activations.Length; a++)
                _layers[0].Neurons[a].Activation = activations[a];
        }

        private void ProcessLayers()
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

        public static MultiLayerPerceptron CreatePerceptron(SerialPerceptron serialPerceptron, int seed)
        {
            MultiLayerPerceptron perceptron = new MultiLayerPerceptron(seed,
                serialPerceptron.LayerParams, serialPerceptron.WeightInitializationType);

            int neuronIndex = 0;
            for (int l = 1; l < perceptron._layers.Length; l++)
            {
                Layer layer = perceptron._layers[l];
                for (int n = 0; n < layer.Neurons.Length; n++, neuronIndex++)
                {
                    Neuron neuron = layer.Neurons[n];
                    SerialNeuron serialNeuron = serialPerceptron.SerialNeurons[neuronIndex];
                    neuron.Weights = (float[])serialNeuron.Weights.Clone();
                    neuron.Bias = serialNeuron.Bias;
                }
            }

            return perceptron;
        }

        public SerialPerceptron ExportPerceptron()
        {
            int neuronsCount = 0;
            for (int l = 1; l < _layers.Length; l++)
                neuronsCount += _layers[l].Neurons.Length;

            SerialNeuron[] serialNeurons = new SerialNeuron[neuronsCount];

            int neuronIndex = 0;
            for (int l = 1; l < _layers.Length; l++)
            {
                Layer layer = _layers[l];
                for (int n = 0; n < layer.Neurons.Length; n++, neuronIndex++)
                {
                    serialNeurons[neuronIndex] = new SerialNeuron(
                        (float[])layer.Neurons[n].Weights.Clone(),
                        layer.Neurons[n].Bias);
                }
            }

            return new SerialPerceptron(_layerParams, serialNeurons, WeightInitialisationType.Manual);
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
}