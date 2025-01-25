public interface IAgentBody
{
    public float[] GetInputActivations();

    public void ActionOutputs(float[] outputs);

    public float GetFitness();
}