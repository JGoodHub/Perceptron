using UnityEngine;

namespace NeuralNet
{
    public class ScriptablePerceptron : ScriptableObject
    {
        [SerializeField] private SerialPerceptron _serialPerceptron;

        public SerialPerceptron SerialPerceptron
        {
            get => _serialPerceptron;
            set => _serialPerceptron = value;
        }
    }
}