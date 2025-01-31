using GoodHub.Core.Runtime.Utils;
using UnityEngine;
using Random = System.Random;

namespace NeuralNet
{
    public enum WeightInitialisationType
    {
        None,
        XavierInitialisation,
        HeInitialization,
        //LeCunInitialisation
    }

    public class WeightInitialisations
    {
        public static float[] XavierInitialisation(Random random, int fanInCount, int fanOutCount)
        {
            float alpha = Mathf.Sqrt(6f / (fanInCount + fanOutCount));

            float[] weights = random.UniformDistribution(fanInCount, -alpha, alpha);

            return weights;
        }

        public static float[] HeInitialization(Random random, int fanInCount)
        {
            float alpha = Mathf.Sqrt(6f / fanInCount);

            float[] weights = random.UniformDistribution(fanInCount, -alpha, alpha);

            return weights;
        }

        // public static float[] LeCunInitialisation(Random random, int fanInCount)
        // {
        //     float[] weights = random.NormalDistribution(0, 1f / fanInCount, fanInCount);
        //     return weights;
        // }
    }
}