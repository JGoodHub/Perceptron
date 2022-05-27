using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class DroneMLP : MonoBehaviour
{
}

public class Perceptron
{
    private List<Layer> layers;

    public Layer InputLayer => layers[0];
    public Layer OutputLayer => layers[layers.Count - 1];

    public Perceptron(List<int> neuronsPerLayer)
    {
        layers = new List<Layer>();

        for (int i = 0; i < neuronsPerLayer.Count; i++)
        {
            layers.Add(new Layer(this, neuronsPerLayer[i]));
        }

        inputLayer = new Layer(inputNeuronCount, false);
        outputLayer = new Layer(outputNeuronCount, false);

        hiddenLayers = new List<Layer>();
        for (int i = 0; i < hiddenLayersCount; i++)
        {
            hiddenLayers.Add(new Layer(hiddenLayerNeuronCount, true));
        }
    }

    public void SetInputLayerActivations(List<float> activations)
    {
        for (int i = 0; i < activations.Count; i++)
        {
            inputLayer.neurons[i].activation = activations[i];
        }
    }

    public List<float> GetOutputLayerActivations()
    {
        return outputLayer.neurons.Select(neuron => neuron.activation).ToList();
    }

    public void Process()
    {
    }


    public class Layer
    {
        public Perceptron perceptron;

        public List<Neuron> neurons;

        public Layer(Perceptron perceptron, int neuronCount)
        {
            this.perceptron = perceptron;

            neurons = new List<Neuron>();
            for (int i = 0; i < neuronCount; i++)
            {
                neurons[i] = new Neuron(this);
            }
        }
    }

    public class Neuron
    {
        public Layer layer;

        public float activation;
        public List<float> weights;
        public float bias;

        public Neuron(Layer layer, int numberOfInputs, Random random)
        {
            this.layer = layer;

            bias = RandomFloat(random, -5, 5);
            weights = new List<float>();
            for (int i = 0; i < numberOfInputs; i++)
            {
                weights[i] = RandomFloat(random, -5, 5);
            }
        }

        public float Activation(List<float> inputs)
        {
            activation = bias;

            for (int i = 0; i < weights.Count; i++)
            {
                activation += weights[i] * inputs[i];
            }

            activation = Sigmoid(activation);
            return activation;
        }
    }

    public static float Sigmoid(float input)
    {
        return (float) (1 / (1 + Math.Exp(-input)));
    }

    public static float RandomFloat(Random random, float min, float max)
    {
        return (float) (max * random.NextDouble() - min);
    }
}