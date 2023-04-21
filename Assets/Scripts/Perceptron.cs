using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class Perceptron
{
    private List<Layer> _layers;
    private Random _random;
    
    public Layer InputLayer => _layers[0];

    public Layer OutputLayer => _layers[_layers.Count - 1];

    public Perceptron(List<int> neuronsPerLayer, Random random = null)
    {
        _random = random;
        _random ??= new Random(DateTime.Now.GetHashCode());

        _layers = new List<Layer>();

        for (int i = 0; i < neuronsPerLayer.Count; i++)
        {
            Layer previousLayer = i == 0 ? null : _layers[i - 1];
            Layer layer = new Layer(this, neuronsPerLayer[i], previousLayer, _random);
            _layers.Add(layer);
        }
    }

    public void SetInputActivations(List<float> activations)
    {
        for (int i = 0; i < activations.Count; i++)
            InputLayer.Neurons[i].Activation = activations[i];
    }

    public List<float> GetOutputActivations()
    {
        List<float> outputActivations = new List<float>();

        foreach (Neuron neuron in OutputLayer.Neurons)
            outputActivations.Add(neuron.Activation);

        return outputActivations;
    }

    public int GetMaxOutputActivationIndex(out float activation)
    {
        List<float> outputActivations = GetOutputActivations();

        int maxIndex = 0;
        activation = float.MinValue;

        for (int i = 0; i < outputActivations.Count; i++)
        {
            if (activation < outputActivations[i])
            {
                maxIndex = i;
                activation = outputActivations[i];
            }
        }

        return maxIndex;
    }

    public void ProcessInputActivations()
    {
        for (int i = 1; i < _layers.Count - 1; i++)
            _layers[i].ComputeActivations();
    }

    public void ApplyRandomWeightAndBiasMutations(float mutationsRange)
    {
        for (int i = 1; i < _layers.Count - 1; i++)
            _layers[i].ApplyRandomWeightAndBiasMutations(_random, mutationsRange);
    }

    public static float Sigmoid(float input)
    {
        return (float)(1f / (1f + Math.Exp(-input)));
    }

    public static float Relu(float input)
    {
        return input <= 0f ? 0f : input;
    }

    public static float RandomFloat(Random random, float min, float max)
    {
        return (float)(max * random.NextDouble() - min);
    }
    
    public Perceptron Clone()
    {
        List<int> neuronsPerLayer = new List<int>();

        foreach (Layer layer in _layers)
            neuronsPerLayer.Add(layer.Neurons.Count);

        Perceptron clone = new Perceptron(neuronsPerLayer, new Random());

        for (int l = 0; l < _layers.Count; l++)
        {
            for (int n = 0; n < _layers[l].Neurons.Count; n++)
            {
                clone._layers[l].Neurons[n].CloneWeightsAndBias(_layers[l].Neurons[n]);
            }
        }
        
        return clone;
    }
}

public class Layer
{
    private Perceptron _perceptron;

    private List<Neuron> _neurons;

    public List<Neuron> Neurons => _neurons;

    public Layer(Perceptron perceptron, int neuronCount, Layer previousLayer, Random random)
    {
        _perceptron = perceptron;

        _neurons = new List<Neuron>();
        for (int i = 0; i < neuronCount; i++)
        {
            Neuron neuron = new Neuron(this, previousLayer == null ? new List<Neuron>() : previousLayer.Neurons, random);
            _neurons.Add(neuron);
        }
    }

    public void ComputeActivations()
    {
        foreach (Neuron neuron in _neurons)
            neuron.ComputeActivation();
    }

    public void ApplyRandomWeightAndBiasMutations(Random random, float mutationsRange)
    {
        foreach (Neuron neuron in _neurons)
        {
            neuron.ApplyRandomWeightAndBiasMutations(random, mutationsRange);
        }
    }
}


public class Neuron
{
    private Layer _layer;

    private List<Neuron> _inputNeurons;
    private List<float> _weights;
    private float _bias;

    private float _activation;

    public float Activation
    {
        get => _activation;
        set => _activation = value;
    }

    public Neuron(Layer layer, List<Neuron> inputNeurons, Random random)
    {
        _layer = layer;

        _inputNeurons = inputNeurons;
        _weights = new List<float>();
        _bias = Perceptron.RandomFloat(random, -5, 5);

        for (int i = 0; i < _inputNeurons.Count; i++)
            _weights.Add(Perceptron.RandomFloat(random, -5, 5));
    }

    public float ComputeActivation()
    {
        _activation = 0;

        for (int i = 0; i < _inputNeurons.Count; i++)
            _activation += _inputNeurons[i].Activation * _weights[i];

        _activation += _bias;
        _activation = Perceptron.Relu(_activation);

        return _activation;
    }

    public void CloneWeightsAndBias(Neuron other)
    {
        _weights.Clear();
        foreach (float otherWeight in other._weights)
        {
            _weights.Add(otherWeight);
        }
        
        _bias = other._bias;
    }

    public void ApplyRandomWeightAndBiasMutations(Random random, float mutationRange)
    {
        for (int i = 0; i < _inputNeurons.Count; i++)
            _weights[i] += Perceptron.RandomFloat(random, -mutationRange, mutationRange);

        _bias = Perceptron.RandomFloat(random, -mutationRange, mutationRange);
    }
}