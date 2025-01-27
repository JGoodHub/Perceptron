using System;
using System.Collections;
using System.Collections.Generic;
using NeuralNet;
using UnityEngine;
using UnityEngine.Serialization;

public class CarAgent : MonoBehaviour, IAgentBody
{
    [Serializable]
    public class ViewRay
    {
        public float Angle;
        public float MaxDistance;
    }

    [Serializable]
    public enum AgentState
    {
        Alive,
        Crashed,
        Timeout,
        Misaligned,
        Finished
    }

    [SerializeField] private List<ViewRay> _viewRays;
    [Space]
    [SerializeField] private float _acceleration = 1;
    [SerializeField] private float _deceleration = 2;
    [SerializeField] private float _resistance = 0.5f;
    [Space]
    [SerializeField] private float _maxSpeed = 4;
    [SerializeField] private AnimationCurve _speedSteeringCurve;
    [Space]
    [SerializeField] private CarGraphicsSwapper _graphics;
    [SerializeField] private bool _playerControlled;

    private float _throttleInput;
    private float _brakingInput;
    private float _steeringInput;

    private float _currentSpeed;
    private float _currentTurning;

    private AgentState _state;
    private float _timeAlive;
    private float _trackProgress;
    private float _toSlowCountdown = 2f;

    private bool _isBestAgent;

    private float _penalty = 0f;

    public AgentState State => _state;

    public CarGraphicsSwapper Graphics => _graphics;

    public void InitialiseGraphics(int seed, int depth)
    {
        _graphics.SelectColourFromSeed(seed);
        _graphics.SetDepth(depth);
    }

    public void ResetBody()
    {
        _trackProgress = 0f;
        _timeAlive = 0f;

        _state = AgentState.Alive;

        _currentSpeed = 0;
        _currentTurning = 0;

        _throttleInput = 0;
        _steeringInput = 0;

        _toSlowCountdown = 2f;

        _penalty = 0f;

        _graphics.ResetPositionHistory();
    }

    public void SetControls(float steering, float throttle, float braking)
    {
        _steeringInput = steering;
        _throttleInput = throttle;
        _brakingInput = braking;
    }

    public float[] GetViewRayDistances()
    {
        float[] distances = new float[_viewRays.Count];

        for (int i = 0; i < _viewRays.Count; i++)
        {
            Vector3 viewRayDirection = Quaternion.Euler(0, 0, -_viewRays[i].Angle) * transform.up;

            float contactDistanceNormalised = 1f;

            const int layerMask = 256; // Environment

            RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, viewRayDirection, _viewRays[i].MaxDistance, layerMask);

            if (raycastHit.collider != null)
                contactDistanceNormalised = raycastHit.distance / _viewRays[i].MaxDistance;

            distances[i] = contactDistanceNormalised;
        }

        return distances;
    }

    public void UpdateWithTime(float deltaTime)
    {
        if (_state != AgentState.Alive)
            return;

        TrackCircuit currentTrack = TrackManager.Singleton.CurrentTrack;

        _currentSpeed += (_acceleration * _throttleInput * deltaTime);
        _currentSpeed -= (_deceleration * _brakingInput * deltaTime);
        _currentSpeed -= (_resistance * deltaTime);

        _currentSpeed = Mathf.Clamp(_currentSpeed, 0, _maxSpeed);

        _currentTurning = _speedSteeringCurve.Evaluate(_currentSpeed) * ((_steeringInput - 0.5f) * 2f);
        transform.Rotate(Vector3.forward, _currentTurning * -1f * deltaTime);

        Vector3 translationVector = transform.up * (_currentSpeed * deltaTime);
        transform.position += translationVector;

        _timeAlive += deltaTime;
        _trackProgress = Mathf.Max(_trackProgress, currentTrack.GetNormalisedDistanceAlongTrack(transform) * 100);

        if (_throttleInput > 0.15f && _brakingInput > 0.15f)
        {
            _penalty += deltaTime;
        }

        if (_playerControlled == false)
        {
            if (_currentSpeed < 0.3f)
            {
                _toSlowCountdown -= deltaTime;
            }
            else
            {
                _toSlowCountdown = 2f;
            }

            if (_toSlowCountdown <= 0f)
            {
                _state = AgentState.Timeout;
            }

            if (currentTrack.IsAlignedToTrack(transform) == false)
            {
                _state = AgentState.Misaligned;
            }
        }

        _graphics.LogPosition();
        _graphics.UpdateGraphics(_throttleInput, _brakingInput, _state != AgentState.Alive, _isBestAgent);
    }

    public void HandleAgentCompleted()
    {
        _graphics.UpdateGraphics(_throttleInput, _brakingInput, _state != AgentState.Alive, _isBestAgent);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_state != AgentState.Alive)
            return;

        if (CompareTag(other.tag))
            return;

        if (other.CompareTag("Finish"))
        {
            _state = AgentState.Finished;
            return;
        }

        _state = AgentState.Crashed;
    }

    private void OnDrawGizmos()
    {
        return;

        foreach (ViewRay viewRay in _viewRays)
        {
            Gizmos.color = Color.green;

            Vector3 viewRayDirection = Quaternion.Euler(0, 0, -viewRay.Angle) * transform.up;
            float contactDistance = viewRay.MaxDistance;

            int layerMask = ~LayerMask.GetMask("Agents", "Finish");
            RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, viewRayDirection, viewRay.MaxDistance, layerMask);

            if (raycastHit.collider != null)
            {
                contactDistance = raycastHit.distance;
                Gizmos.color = Color.red;
            }

            Gizmos.DrawRay(transform.position, viewRayDirection * contactDistance);
        }
    }

    public void SetBestAgent(bool isBestAgent)
    {
        _isBestAgent = isBestAgent;
    }

    public float[] GetInputActivations()
    {
        float[] inputActivations = new float[12];

        inputActivations[0] = _throttleInput;
        inputActivations[1] = _brakingInput;
        inputActivations[2] = _steeringInput;
        inputActivations[3] = _currentSpeed / _maxSpeed;
        inputActivations[4] = _currentSpeed / 120;

        GetViewRayDistances().CopyTo(inputActivations, 5);

        return inputActivations;
    }

    public void ActionOutputs(float[] outputs)
    {
        _steeringInput = Mathf.Clamp(outputs[0], -1000, 1000);
        _throttleInput = Mathf.Clamp(outputs[1], -1000, 1000);
        _brakingInput = Mathf.Clamp(outputs[2], -1000, 1000);
    }

    public float GetFitness()
    {
        if (_state == AgentState.Finished)
        {
            return 100 + (50 - _timeAlive) - _penalty;
        }

        return _trackProgress;
    }
}