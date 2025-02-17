using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class NeuronElement : MonoBehaviour
{

    [SerializeField] private Text _activationText;

    [SerializeField] private RectTransform _connectionsInput;
    [SerializeField] private RectTransform _connectionsOutput;

    public RectTransform ConnectionsInput => _connectionsInput;

    public RectTransform ConnectionsOutput => _connectionsOutput;

    public void Initialise(float activation, float bias)
    {
        _activationText.text = $"A:{activation} | B:{bias}";
    }

}
