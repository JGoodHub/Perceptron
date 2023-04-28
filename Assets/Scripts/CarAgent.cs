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
    [SerializeField] private bool _playerControlled;

    private float _throttleInput;
    private float _steeringInput;

    private float _currentSpeed;
    private float _currentTurning;

    private Vector2 _lastFramePosition;
    
    private void Start()
    {
        _lastFramePosition = transform.position;
    }
    
    public void SetThrottle(float throttle)
    {
        _throttleInput = throttle;
    }

    public void SetSteering(float steering)
    {
        _steeringInput = steering;
    }

    private void Update()
    {
        if (_playerControlled)
        {
            SetThrottle(Input.GetAxis("Vertical"));
            SetSteering(Input.GetAxis("Horizontal"));
        }

        Vector2 position2D = transform.position;

        _currentSpeed = (position2D - _lastFramePosition).magnitude / Time.deltaTime;
        float accelerationAtSpeed = _accelerationCurve.Evaluate(_currentSpeed / _maxSpeed);
        _currentSpeed += _throttleInput * (_maxAcceleration * accelerationAtSpeed);

        if (_throttleInput <= 0)
            _currentSpeed -= _maxSpeed * Time.deltaTime;
        
        _currentSpeed = Mathf.Clamp(_currentSpeed, 0, _maxSpeed);

        _currentTurning = _maxSteering * _steeringInput;
        transform.Rotate(Vector3.forward, _currentTurning * -1f);

        _lastFramePosition = transform.position;
        transform.position += transform.up * (_currentSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        foreach (ViewRay viewRay in _viewRays)
        {
            Gizmos.color = Color.green;

            Vector3 viewRayDirection = Quaternion.Euler(0, 0, -viewRay.Angle) * transform.up;
            float contactDistance = viewRay.MaxDistance;

            RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, viewRayDirection, viewRay.MaxDistance);
            if (raycastHit.distance > 0)
            {
                contactDistance = raycastHit.distance;
                Gizmos.color = Color.red;
            }

            Gizmos.DrawRay(transform.position, viewRayDirection * contactDistance);
        }
    }
}