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

    public Perceptron(List<int> neuronsPerLayer, int seed = -1)
    {
        _random = new Random(seed == -1 ? DateTime.Now.GetHashCode() : seed);
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

    public void ProcessInputActivations()
    {
        for (int i = 1; i < _layers.Count - 1; i++)
            _layers[i].ComputeActivations();
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
        _bias = RandomFloat(random, -5, 5);

        for (int i = 0; i < _inputNeurons.Count; i++)
            _weights.Add(RandomFloat(random, -5, 5));
    }

    public float ComputeActivation()
    {
        _activation = 0;

        for (int i = 0; i < _inputNeurons.Count; i++)
            _activation += _inputNeurons[i].Activation * _weights[i];

        _activation += _bias;
        _activation = Relu(_activation);

        return _activation;
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
}