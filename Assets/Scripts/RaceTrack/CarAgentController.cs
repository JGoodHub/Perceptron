using NeuralNet;
using UnityEngine;

public class CarAgentController : MonoBehaviour
{
    [SerializeField] private ScriptablePerceptron _scriptablePerceptron;
    [SerializeField] private CarAgent _carAgent;

    private MultiLayerPerceptron _perceptron;

    private void Start()
    {
        _perceptron = MultiLayerPerceptron.CreatePerceptron(_scriptablePerceptron.SerialPerceptron, 0);

        _carAgent.ResetBody();
        _carAgent.InitialiseGraphics(0, 1);
    }

    private void FixedUpdate()
    {
        float[] inputActivations = _carAgent.GetInputActivations();
        float[] outputActivations = _perceptron.Process(inputActivations);
        _carAgent.ActionOutputs(outputActivations);

        _carAgent.UpdateWithTime(Time.fixedDeltaTime);
    }
}