using NeuralNet;

public class AgentEntity<TAgentBody> where TAgentBody : IAgentBody
{
    private Perceptron _perceptron;
    private TAgentBody _agentBody;
    private float _fitness;

    public Perceptron Perceptron => _perceptron;

    public TAgentBody AgentBody => _agentBody;

    public float Fitness => _agentBody.GetFitness();

    public AgentEntity(Perceptron perceptron)
    {
        _perceptron = perceptron;
    }

    public void AssignBrain(Perceptron perceptron)
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