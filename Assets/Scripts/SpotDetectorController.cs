using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpotDetectorController : MonoBehaviour
{

    [SerializeField] private int _seed;
    [Space]
    [SerializeField] private RectTransform _spotRect;
    [Space] 
    [SerializeField] private int _hiddenLayerNeurons = 20;
    
    private Perceptron _perceptron;

    private int _correctGuesses;
    private int _incorrectGuesses;

    private int _guessesTotal;

    private Vector2 _spotPosition;
    private int _spotLocation;
    
    private void Start()
    {
        List<int> neuronCounts = new List<int>
        {
            2, _hiddenLayerNeurons, 4
        };

        Random.InitState(_seed);
        _perceptron = new Perceptron(neuronCounts, _seed);
    }

    private void Update()
    {
        RandomiseSpot();
        
        _perceptron.SetInputActivations(new List<float> { _spotPosition.x * 0.001f, _spotPosition.y * 0.001f });
        
        _perceptron.ProcessInputActivations();

        List<float> outputActivations = _perceptron.GetOutputActivations();

        float maxActivation = float.MinValue;
        int maxIndex = 0;
        for (int i = 0; i < outputActivations.Count; i++)
        {
            if (maxActivation < outputActivations[i])
            {
                maxIndex = i;
                maxActivation = outputActivations[i];
            }
        }

        _guessesTotal++;

        if (_spotLocation == maxIndex)
            _correctGuesses++;
        else
            _incorrectGuesses++;
    }

    private void RandomiseSpot()
    {
        _spotPosition = new Vector2(Random.Range(0, 1000), Random.Range(0, 1000));
        _spotRect.anchoredPosition = _spotPosition;

        if (_spotPosition.x < 500)
        {
            _spotLocation = _spotPosition.y < 500 ? 2 : 0;
        }
        else
        {
            _spotLocation = _spotPosition.y < 500 ? 3 : 1;
        }
    }
    
    
}
