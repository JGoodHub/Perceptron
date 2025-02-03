using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeuralNet
{
    [Serializable]
    public class SerialNeuron
    {
        public float[] Weights;
        public float Bias;

        public SerialNeuron(float[] weights, float bias)
        {
            Weights = weights;
            Bias = bias;
        }
    }

    [Serializable]
    public class SerialPerceptron
    {
        public LayerParams[] LayerParams;
        public SerialNeuron[] SerialNeurons;
        public WeightInitialisationType WeightInitializationType;

        public SerialPerceptron(LayerParams[] layerParams, SerialNeuron[] serialNeurons,
            WeightInitialisationType weightInitializationType)
        {
            LayerParams = layerParams;
            SerialNeurons = serialNeurons;
            WeightInitializationType = weightInitializationType;
        }

        public override string ToString()
        {
            return $"Hashcode:{GetHashCode()}";
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            for (int i = 0; i < LayerParams.Length; i++)
            {
                hashCode.Add(LayerParams[i].GetHashCode());
            }

            for (int n = 0; n < SerialNeurons.Length; n++)
            {
                hashCode.Add(SerialNeurons[n].GetHashCode());
            }

            return hashCode.ToHashCode();
        }
    }
}