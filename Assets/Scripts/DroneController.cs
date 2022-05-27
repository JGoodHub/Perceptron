using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    [SerializeField] private bool _playerControlled;

    [SerializeField] private float _primaryThrusterAcceleration;
    [SerializeField] private float _secondaryThrusterAcceleration;

    private Vector3 _velocity;

    [SerializeField] private ParticleSystem _upExhaust;
    [SerializeField] private ParticleSystem _leftExhaust;
    [SerializeField] private ParticleSystem _rightExhaust;
    [SerializeField] private ParticleSystem _downExhaust;

    private bool _upExhaustOn;
    private bool _leftExhaustOn;
    private bool _rightExhaustOn;
    private bool _downExhaustOn;

    private void Update()
    {
        _upExhaustOn = _leftExhaustOn = _rightExhaustOn = _downExhaustOn = false;

        if (_playerControlled)
        {
            if (Input.GetKey(KeyCode.A))
            {
                FireThruster(ThrusterDirection.Right, Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.D))
            {
                FireThruster(ThrusterDirection.Left, Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.W))
            {
                FireThruster(ThrusterDirection.Down, Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.S))
            {
                FireThruster(ThrusterDirection.Up, Time.deltaTime);
            }
        }
        
        UpdateAllExhaustsParticleSystems();
    }

    private void LateUpdate()
    {
        UpdatePosition(Time.deltaTime);
    }

    public void UpdatePosition(float timeStep)
    {
        _velocity += Physics.gravity * timeStep;
        transform.position += _velocity * timeStep;
    }

    public void FireThruster(ThrusterDirection direction, float timeStep)
    {
        switch (direction)
        {
            case ThrusterDirection.Up:
                _velocity += Vector3.down * _secondaryThrusterAcceleration * timeStep;
                _upExhaustOn = true;
                break;
            case ThrusterDirection.Left:
                _velocity += Vector3.right * _secondaryThrusterAcceleration * timeStep;
                _leftExhaustOn = true;
                break;
            case ThrusterDirection.Right:
                _velocity += Vector3.left * _secondaryThrusterAcceleration * timeStep;
                _rightExhaustOn = true;
                break;
            case ThrusterDirection.Down:
                _velocity += Vector3.up * _primaryThrusterAcceleration * timeStep;
                _downExhaustOn = true;
                break;
        }
    }

    private void UpdateAllExhaustsParticleSystems()
    {
        if (_upExhaustOn && _upExhaust.isPlaying == false)
            _upExhaust.Play();
        else if (_upExhaustOn == false)
            _upExhaust.Stop();
        
        if (_leftExhaustOn && _leftExhaust.isPlaying == false)
            _leftExhaust.Play();
        else if (_leftExhaustOn == false)
            _leftExhaust.Stop();
        
        if (_rightExhaustOn && _rightExhaust.isPlaying == false)
            _rightExhaust.Play();
        else if (_rightExhaustOn == false)
            _rightExhaust.Stop();
        
        if (_downExhaustOn && _downExhaust.isPlaying == false)
            _downExhaust.Play();
        else if (_downExhaustOn == false)
            _downExhaust.Stop();
    }
}

public enum ThrusterDirection
{
    Up,
    Left,
    Right,
    Down
}