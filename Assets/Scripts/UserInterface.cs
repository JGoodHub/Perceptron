using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    
    public static UserInterface Instance;

    private void Awake() => Instance = this;

    [SerializeField] private Text _generationCounter;
    [SerializeField] private Text _bestGenerationFitness;
    [SerializeField] private Toggle _frameToggle;
    [SerializeField] private Toggle _compToggle;

    private void Start()
    {
        _frameToggle.onValueChanged.AddListener(ToggleYield);
        _compToggle.onValueChanged.AddListener(ToggleComp);
        
       _frameToggle.isOn = FindObjectOfType<CarAgentTrainer>().YieldEveryFrame;
       _compToggle.isOn = FindObjectOfType<CarAgentTrainer>().YieldOnComplete;
    }

    public void UpdateText(int generationIndex, float bestFitness)
    {
        _generationCounter.text = $"Generation: {generationIndex}";
        _bestGenerationFitness.text = $"Best Fitness: {bestFitness}";
    }

    public void ToggleYield(bool state)
    {
        FindObjectOfType<CarAgentTrainer>().YieldEveryFrame = state;
    }
    
    public void ToggleComp(bool state)
    {
        FindObjectOfType<CarAgentTrainer>().YieldOnComplete = state;
    }

}
