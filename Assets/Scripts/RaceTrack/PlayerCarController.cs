using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    [SerializeField] private CarAgent _carAgent;

    private void Start()
    {
        _carAgent.ResetBody();
    }

    private void Update()
    {
        float steering = (Input.GetAxis("Horizontal") + 1f) / 2f;
        float throttleAndBrake = Input.GetAxis("Vertical");

        float throttle = Mathf.Clamp01(throttleAndBrake);
        float brake = Mathf.Clamp01(-throttleAndBrake);
        
        _carAgent.SetControls(steering, throttle, brake);
    }

    private void FixedUpdate()
    {
        _carAgent.UpdateWithTime(Time.fixedDeltaTime);
    }
}
