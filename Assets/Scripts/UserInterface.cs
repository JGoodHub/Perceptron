using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    
    public static UserInterface Instance;

    private void Awake() => Instance = this;



    [SerializeField] private Text _generationCounter;
    [SerializeField] private Text _bestGenerationFitness;

    public void UpdateText(int generationIndex, float bestFitness)
    {
        _generationCounter.text = $"Generation: {generationIndex}";
        _bestGenerationFitness.text = $"Best Fitness: {bestFitness}";
    }

}
