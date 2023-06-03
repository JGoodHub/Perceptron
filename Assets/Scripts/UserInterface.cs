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
    [SerializeField] private Slider _timeSlider;
    [SerializeField] private TextMeshProUGUI _timeScaleText;

    private float _fixedDeltaTime;

    private void Start()
    {
        _timeSlider.onValueChanged.AddListener(TimeSliderValueChanged);
        _fixedDeltaTime = Time.fixedDeltaTime;
    }

    public void UpdateText(int generationIndex, float bestFitness)
    {
        _generationCounter.text = $"Generation: {generationIndex}";
        _bestGenerationFitness.text = $"Best Fitness: {bestFitness}";
    }

    
    private void TimeSliderValueChanged(float newValue)
    {
        Time.timeScale = newValue;
        Time.fixedDeltaTime = _fixedDeltaTime * Time.timeScale;

        _timeScaleText.text = "x" + newValue;
    }

}
