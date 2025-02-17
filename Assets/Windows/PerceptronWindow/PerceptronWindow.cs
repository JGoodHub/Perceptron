using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoodHub.Core.Runtime;
using NeuralNet;
using UnityEngine;
using UnityEngine.UI;

public class PerceptronWindow : Window
{
    [SerializeField] private ScriptablePerceptron _testPerceptron;

    [SerializeField] private Transform _layersRoot;
    [SerializeField] private GameObject _layerElementPrefab;
    [SerializeField] private GameObject _neuronElementPrefab;

    [SerializeField] private Transform _connectionsRoot;
    [SerializeField] private GameObject _connectionElementPrefab;

    private List<GameObject> _layerElements = new List<GameObject>();
    private List<List<NeuronElement>> _neuronElements = new List<List<NeuronElement>>();
    private List<List<ConnectionElement>> _connectionElements = new List<List<ConnectionElement>>();

    protected override void OnInitialised()
    {
        LayerParams[] layerParams = _testPerceptron.SerialPerceptron.LayerParams;

        for (int l = 0; l < layerParams.Length; l++)
        {
            GameObject layerElement = Instantiate(_layerElementPrefab, _layersRoot);
            _layerElements.Add(layerElement);
        }

        int neuronIndex = 0;
        for (int l = 0; l < layerParams.Length; l++)
        {
            List<NeuronElement> layerNeuronElements = new List<NeuronElement>();

            for (int n = 0; n < layerParams[l].NeuronCount; n++)
            {
                NeuronElement neuronElement = Instantiate(_neuronElementPrefab, _layerElements[l].transform).GetComponent<NeuronElement>();
                neuronElement.Initialise(0, 0);

                layerNeuronElements.Add(neuronElement);

                if (l >= 1)
                {
                    SerialNeuron serialNeuron = _testPerceptron.SerialPerceptron.SerialNeurons[neuronIndex];
                    neuronIndex++;

                    neuronElement.Initialise(0, serialNeuron.Bias);

                    List<ConnectionElement> connections = new List<ConnectionElement>();

                    for (int w = 0; w < serialNeuron.Weights.Length; w++)
                    {
                        ConnectionElement connection = Instantiate(_connectionElementPrefab, _connectionsRoot).GetComponent<ConnectionElement>();
                        connections.Add(connection);

                        connection.Initialise(_neuronElements[l - 1][w], neuronElement, serialNeuron.Weights[w]);
                    }

                    _connectionElements.Add(connections);
                }
            }

            _neuronElements.Add(layerNeuronElements);
        }
    }
}