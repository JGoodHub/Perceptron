using System;
using System.Collections.Generic;

namespace NeuralNet
{
    [Serializable]
    public class SerialPerceptron
    {
        public readonly List<int> NeuronsPerLayer;
        public readonly List<float[]> Weights;
        public readonly List<float> Biases;
        public readonly ActivationFunctions.ActivationFunctionType ActivationFunctionType;

        public SerialPerceptron(List<int> neuronsPerLayer, List<float[]> weights, List<float> biases,
            ActivationFunctions.ActivationFunctionType activationFunctionType)
        {
            NeuronsPerLayer = neuronsPerLayer;
            Weights = weights;
            Biases = biases;
            ActivationFunctionType = activationFunctionType;
        }

        public override string ToString()
        {
            return $"Hashcode:{GetHashCode()}";
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            for (int i = 0; i < NeuronsPerLayer.Count; i++)
                hashCode.Add(NeuronsPerLayer[i]);

            for (int n = 0; n < Weights.Count; n++)
            {
                for (int w = 0; w < Weights[n].Length; w++)
                {
                    hashCode.Add(Weights[n][w]);
                }

                hashCode.Add(Biases[n]);
            }

            return hashCode.ToHashCode();
        }
    }
}