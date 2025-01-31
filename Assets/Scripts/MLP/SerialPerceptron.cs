using System;
using System.Collections.Generic;

namespace NeuralNet
{
    [Serializable]
    public class SerialPerceptron
    {
        private int _neuronCount = 0;
        
        public readonly LayerParams[] LayerParams;
        public readonly float[][] Weights;
        public readonly float[] Biases;
        public readonly WeightInitialisationType WeightInitializationType;
        
        public SerialPerceptron(LayerParams[] layerParams,
            float[][] weights, float[] biases,
            WeightInitialisationType weightInitializationType)
        {
            LayerParams = layerParams;
            Weights = weights;
            Biases = biases;
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
                hashCode.Add(LayerParams[i].GetHashCode());

            for (int n = 0; n < Weights.Length; n++)
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