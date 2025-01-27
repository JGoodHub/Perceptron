using UnityEngine;

namespace NeuralNet
{
    [CreateAssetMenu(fileName = "PerceptronModel", menuName = "Perceptron/New Perceptron Model", order = 0)]
    public class PerceptronModel : ScriptableObject
    {

        public SerialPerceptron SerialPerceptron;




    }
}