using NeuralNet;

public class AgentTracker
{
    public MultiLayerPerceptron perceptron;
    public float fitness;

    public AgentTracker(MultiLayerPerceptron perceptron)
    {
        this.perceptron = perceptron;
    }

    public override string ToString()
    {
        return $"Per:({perceptron}) | (Fit:{fitness})";
    }
}