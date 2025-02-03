using NeuralNet;

public class AgentEntity<TAgentBody> where TAgentBody : IAgentBody
{
    private MultiLayerPerceptron _perceptron;
    private TAgentBody _agentBody;
    private float _fitness;

    public MultiLayerPerceptron Perceptron => _perceptron;

    public TAgentBody AgentBody => _agentBody;

    public float Fitness => _agentBody.GetFitness();

    public AgentEntity(MultiLayerPerceptron perceptron)
    {
        _perceptron = perceptron;
    }

    public void AssignBrain(MultiLayerPerceptron perceptron)
    {
        _perceptron = perceptron;
    }

    public void AssignBody(TAgentBody agentBody)
    {
        _agentBody = agentBody;
    }

    public void UpdateBrain()
    {
        float[] inputActivations = _agentBody.GetInputActivations();
        float[] outputActivations = _perceptron.Process(inputActivations);
        _agentBody.ActionOutputs(outputActivations);
    }
}