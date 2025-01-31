using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAgent : MonoBehaviour, IAgentBody
{
    [Serializable]
    private class Thruster
    {
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _forcePosition;
        [SerializeField] private SpriteRenderer _exhaustSprite;
        [Space]
        [SerializeField] private float _maxThrust;
        [SerializeField] private float _maxRotation;

        private float _thrustInput;
        private float _rotationInput;

        public Transform Pivot => _pivot;

        public Transform ForcePosition => _forcePosition;

        public SpriteRenderer ExhaustSprite => _exhaustSprite;

        public float MaxThrust => _maxThrust;

        public float MaxRotation => _maxRotation;

        public float ThrustInput
        {
            get => _thrustInput;
            set => _thrustInput = value;
        }

        public float RotationInput
        {
            get => _rotationInput;
            set => _rotationInput = value;
        }
    }

    [SerializeField] private Rigidbody2D _rigidbody;

    [SerializeField] private Thruster _leftThruster;
    [SerializeField] private Thruster _rightThruster;
    
    private float _timeAlive;

    
    public void ResetBody()
    {
        _timeAlive = 0f;
        
        _leftThruster.Pivot.rotation = Quaternion.identity;
        _rightThruster.Pivot.rotation = Quaternion.identity;
    }

    public void UpdateWithTime(float deltaTime)
    {
        // _leftThruster.Pivot.Rotate(Vector3.forward, _testLeftThrusterRotationInput * Time.fixedDeltaTime);
        // _thrusterRightPivot.Rotate(Vector3.forward, _testRightThrusterRotationInput * Time.fixedDeltaTime);
        //
        // _rigidbody.AddForceAtPosition(_thrusterLeftForcePosition.up * _testLeftThrusterForce, _thrusterLeftForcePosition.position, ForceMode2D.Force);
        // _rigidbody.AddForceAtPosition(_thrusterRightForcePosition.up * _testRightThrusterForce, _thrusterRightForcePosition.position, ForceMode2D.Force);
        //
        // _leftThruster.ExhaustSprite.color = new Color(1f, 1f, 1f, _testLeftThrusterForce / _leftThruster.MaxThrust);
    }

    public float[] GetInputActivations()
    {
        // Inputs:
        // 0: Torque
        // 1: Vel X
        // 2: Vel Y
        // 3: Thruster-L Angle
        // 4: Thruster-L Thrust
        // 5: Thruster-R Angle
        // 6: Thruster-R Thrust
        // 7: Target Node X
        // 8: Target Node Y
        // 9: Target Node Dist

        float[] inputActivations = new float[10];

        return inputActivations;
    }

    public void ActionOutputs(float[] outputs)
    {
        // Outputs:
        // 0: Thruster-L Rotation Input
        // 1: Thruster-L Thrust Input
        // 2: Thruster-R Rotation Input
        // 3: Thruster-R Thrust Input
        
       // _leftThruster.ThrustInput = 
    }

    public float GetFitness()
    {
        return _timeAlive;
    }

}