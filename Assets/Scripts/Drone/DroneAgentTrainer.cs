using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GoodHub.Core.Runtime;
using UnityEngine;

public class DroneAgentTrainer : SceneSingleton<DroneAgentTrainer>
{
    [SerializeField] private TrainingParameters _parameters;

    private AgentsPool<DroneAgent> _agentsPool;
    private int _generationIndex;

    private ITrainingEnvironment _activeTrainingEnvironment;

    private Dictionary<ITrainingEnvironment, List<Vector2>> _fitnessData = new Dictionary<ITrainingEnvironment, List<Vector2>>();

    public event Action OnFitnessDataUpdated;

    public List<Vector2> ActiveEnvironmentFitnessData
    {
        get
        {
            if (_activeTrainingEnvironment == null)
                return new List<Vector2>();

            if (_fitnessData.TryGetValue(_activeTrainingEnvironment, out List<Vector2> fitnessData))
                return fitnessData;

            return new List<Vector2>();
        }
    }

    private void Start()
    {
        _agentsPool = new AgentsPool<DroneAgent>(_parameters);

        SpawnDroneAgents();
    }

    private void SpawnDroneAgents() { }

    private async UniTask TrainingTask()
    {
        
    }
}