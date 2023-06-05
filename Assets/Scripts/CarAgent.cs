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

    [SerializeField] private List<ViewRay> _viewRays;

    [SerializeField] private AnimationCurve _accelerationCurve;
    [SerializeField] private float _maxAcceleration;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _maxSteering;
    [Space]
    [SerializeField] private CarGraphicsSwapper _graphics;

    private float _throttleInput;
    private float _steeringInput;

    private float _currentSpeed;
    private float _currentTurning;

    private bool _crashed;
    private bool _finished;
    private float _timeAlive;
    private float _trackProgress;
    private float _distanceTravelled;
    private float _toSlowCountdown = 2f;


    public float SpeedNormalised => _currentSpeed / _maxSpeed;

    public float SteeringInput => _steeringInput;

    public bool Crashed => _crashed;

    public bool Finished => _finished;

    public float TimeAlive => _timeAlive;

    public float TrackProgress => _trackProgress;

    public float DistanceTravelled => _distanceTravelled;

    public void InitialiseGraphics(int seed)
    {
        _graphics.SelectColourFromSeed(seed);
    }

    public void ResetAgent()
    {
        _trackProgress = 0f;
        _timeAlive = 0f;
        _distanceTravelled = 0f;

        _crashed = false;
        _finished = false;

        _currentSpeed = 0;
        _currentTurning = 0;

        _throttleInput = 0;
        _steeringInput = 0;

        _toSlowCountdown = 2f;

        _graphics.ResetPositionHistory();
    }

    public void SetThrottle(float throttle)
    {
        _throttleInput = throttle;
    }

    public void SetSteering(float steering)
    {
        _steeringInput = steering;
    }

    public List<float> GetViewRayDistances()
    {
        List<float> distances = new List<float>();

        foreach (ViewRay viewRay in _viewRays)
        {
            Vector3 viewRayDirection = Quaternion.Euler(0, 0, -viewRay.Angle) * transform.up;

            float contactDistanceNormalised = 1f;

            int layerMask = ~LayerMask.GetMask("Agents");
            RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, viewRayDirection, viewRay.MaxDistance, layerMask);

            if (raycastHit.collider != null)
                contactDistanceNormalised = raycastHit.distance / viewRay.MaxDistance;

            distances.Add(contactDistanceNormalised);
        }

        return distances;
    }

    public void UpdateWithTime(float deltaTime)
    {
        float accelerationAtSpeed = _accelerationCurve.Evaluate(_currentSpeed / _maxSpeed);
        
        _currentSpeed += (_maxAcceleration * accelerationAtSpeed) * _throttleInput;
        _currentSpeed -= (_maxSpeed * deltaTime) * (1f - _throttleInput);

        _currentSpeed = Mathf.Clamp(_currentSpeed, 0, _maxSpeed);

        _currentTurning = _maxSteering * ((_steeringInput - 0.5f) * 2f);
        transform.Rotate(Vector3.forward, _currentTurning * -1f * deltaTime);

        Vector3 translationVector = transform.up * (_currentSpeed * deltaTime);
        transform.position += translationVector;

        _distanceTravelled += translationVector.magnitude;
        _timeAlive += deltaTime;
        _trackProgress = Mathf.Max(_trackProgress, TrackCircuit.Instance.GetDistanceAlongTrack(transform));

        if (_currentSpeed < 0.4f)
            _toSlowCountdown -= deltaTime;
        else
            _toSlowCountdown = 2f;

        if (TrackCircuit.Instance.IsAlignedToTrack(transform) == false || _toSlowCountdown <= 0f)
        {
            _crashed = true;
        }

        _graphics.LogPosition();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (CompareTag(other.tag))
            return;

        if (other.CompareTag("Finish"))
        {
            _finished = true;
            return;
        }

        _crashed = true;
    }

    private void OnDrawGizmos()
    {
        return;

        foreach (ViewRay viewRay in _viewRays)
        {
            Gizmos.color = Color.green;

            Vector3 viewRayDirection = Quaternion.Euler(0, 0, -viewRay.Angle) * transform.up;
            float contactDistance = viewRay.MaxDistance;

            int layerMask = ~LayerMask.GetMask("Agents");
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