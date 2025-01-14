using System;
using System.Collections;
using System.Collections.Generic;
using NeuralNet;
using UnityEngine;
using UnityEngine.Serialization;

public class CarAgent : MonoBehaviour
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
        Timeout,
        Crashed,
        Finished
    }

    [SerializeField] private List<ViewRay> _viewRays;

    [SerializeField] private float _acceleration;
    [SerializeField] private float _decceleration;
    [Space]
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _maxSteering;
    [Space]
    [SerializeField] private CarGraphicsSwapper _graphics;

    private float _throttleInput;
    private float _brakingInput;
    private float _steeringInput;

    private float _currentSpeed;
    private float _currentTurning;

    private AgentState _state;
    private float _timeAlive;
    private float _trackProgress;
    private float _distanceTravelled;
    private float _toSlowCountdown = 2f;

    public AgentState State => _state;

    public float SpeedNormalised => _currentSpeed / _maxSpeed;

    public float SteeringInput => _steeringInput;

    public float TimeAlive => _timeAlive;

    public float TrackProgress => _trackProgress;

    public float DistanceTravelled => _distanceTravelled;

    public void InitialiseGraphics(int seed, int depth)
    {
        _graphics.SelectColourFromSeed(seed);
        _graphics.SetDepth(depth);
    }

    public void ResetAgent()
    {
        _trackProgress = 0f;
        _timeAlive = 0f;
        _distanceTravelled = 0f;

        _state = AgentState.Alive;

        _currentSpeed = 0;
        _currentTurning = 0;

        _throttleInput = 0;
        _steeringInput = 0;

        _toSlowCountdown = 2f;

        _graphics.ResetPositionHistory();
    }

    public void SetControls(float steering, float throttle, float braking)
    {
        _steeringInput = steering;
        _throttleInput = throttle;
        _brakingInput = braking;
    }
    
    public List<float> GetViewRayDistances()
    {
        List<float> distances = new List<float>();

        foreach (ViewRay viewRay in _viewRays)
        {
            Vector3 viewRayDirection = Quaternion.Euler(0, 0, -viewRay.Angle) * transform.up;

            float contactDistanceNormalised = 1f;

            int layerMask = ~LayerMask.GetMask("Agents", "Finish");
            RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, viewRayDirection, viewRay.MaxDistance, layerMask);

            if (raycastHit.collider != null)
                contactDistanceNormalised = raycastHit.distance / viewRay.MaxDistance;

            distances.Add(contactDistanceNormalised);
        }

        return distances;
    }

    public void UpdateWithTime(float deltaTime)
    {
        if (_state != AgentState.Alive)
            return;

        _currentSpeed += (_acceleration * _throttleInput);
        _currentSpeed -= (_decceleration * _brakingInput);

        _currentSpeed = Mathf.Clamp(_currentSpeed, 0, _maxSpeed);

        _currentTurning = _maxSteering * ((_steeringInput - 0.5f) * 2f);
        transform.Rotate(Vector3.forward, _currentTurning * -1f * deltaTime);

        Vector3 translationVector = transform.up * (_currentSpeed * deltaTime);
        transform.position += translationVector;

        _distanceTravelled += translationVector.magnitude;
        _timeAlive += deltaTime;
        _trackProgress = Mathf.Max(_trackProgress, TrackManager.Singleton.CurrentTrack.GetNormalisedDistanceAlongTrack(transform) * 100);

        if (_currentSpeed < 0.3f)
            _toSlowCountdown -= deltaTime;
        else
            _toSlowCountdown = 2f;

        if (TrackManager.Singleton.CurrentTrack.IsAlignedToTrack(transform) == false || _toSlowCountdown <= 0f)
        {
            _state = AgentState.Timeout;
        }

        _graphics.LogPosition();
        _graphics.UpdateLights(_throttleInput, _brakingInput);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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
        //return;

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
}