using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoodHub.Core.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class GraphWindow : Window
{
    [SerializeField] private Text _generationText;
    [SerializeField] private Text _bestFitnessText;
    [SerializeField] private GraphElement _graph;
    [SerializeField] private Toggle _yieldEveryFrameToggle;
    [SerializeField] private Toggle _yieldEveryAgentToggle;
    [SerializeField] private Toggle _yieldEveryGenerationToggle;

    public override void Initialise(WindowsManager windowsManager, WindowConfiguration windowConfig, WindowIcon windowIcon)
    {
        base.Initialise(windowsManager, windowConfig, windowIcon);

        _graph.Initialise("Fitness Graph", "Generation", 20, "Fitness", 20);

        _yieldEveryFrameToggle.isOn = CarAgentTrainer.Singleton.YieldEveryFrame;
        _yieldEveryAgentToggle.isOn = CarAgentTrainer.Singleton.YieldEveryAgent;

        _yieldEveryFrameToggle.onValueChanged.AddListener(EveryFrameToggleChanged);
        _yieldEveryAgentToggle.onValueChanged.AddListener(EveryAgentToggleChanged);

        UpdateGraphBodyData();

        CarAgentTrainer.Singleton.OnFitnessDataUpdated += UpdateGraphBodyData;

        OnWindowClosed += Cleanup;
    }

    private void EveryFrameToggleChanged(bool value)
    {
        CarAgentTrainer.Singleton.YieldEveryFrame = value;
    }

    private void EveryAgentToggleChanged(bool value)
    {
        CarAgentTrainer.Singleton.YieldEveryAgent = value;
    }

    private void UpdateGraphBodyData()
    {
        List<Vector2> activeEnvironmentFitnessData = CarAgentTrainer.Singleton.ActiveEnvironmentFitnessData;

        _generationText.text = $"Generation: {activeEnvironmentFitnessData.Count}";
        _bestFitnessText.text = $"Best Fitness: {activeEnvironmentFitnessData.Select(item => item.y).Max()}";

        _graph.SetBodyData(activeEnvironmentFitnessData);
    }

    private void Cleanup(Window closedWindow)
    {
        CarAgentTrainer.Singleton.OnFitnessDataUpdated -= UpdateGraphBodyData;
    }
}