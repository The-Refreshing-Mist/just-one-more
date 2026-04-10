using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentBrakeForce;
    private bool isBraking;

    [SerializeField] private Transform steeringWheel;
    [SerializeField] private float steeringWheelTurnMultiplier = 3f;

    private float steeringWheelAngle;
    [SerializeField] private float steeringWheelSmoothSpeed = 5f;
    [SerializeField] private float motorForce;
    [SerializeField] private float brakeForce;
    [SerializeField] private float maxSteerAngle;

    [SerializeField] private WheelCollider Collider_FL;
    [SerializeField] private WheelCollider Collider_FR;
    [SerializeField] private WheelCollider Collider_RL;
    [SerializeField] private WheelCollider Collider_RR;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    private Quaternion steeringWheelStartRotation;
    private Rigidbody rb;

    private void Start()
    {
        steeringWheelStartRotation = steeringWheel.localRotation;
         rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.8f, 0f);
    }


    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            horizontalInput = -1f;
        }
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            horizontalInput = 1f;
        }

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            verticalInput = 1f;
        }
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            verticalInput = -1f;
        }

        isBraking = Keyboard.current.spaceKey.isPressed;
    }

    private void HandleMotor()
    {
        Collider_FL.motorTorque = verticalInput * motorForce;
        Collider_FR.motorTorque = verticalInput * motorForce;

        currentBrakeForce = isBraking ? brakeForce : 0f;
        ApplyBraking();
    }

    private void ApplyBraking()
    {
        Collider_FL.brakeTorque = currentBrakeForce;
        Collider_FR.brakeTorque = currentBrakeForce;
        Collider_RL.brakeTorque = currentBrakeForce;
        Collider_RR.brakeTorque = currentBrakeForce;
    }

    private void HandleSteering()
    {
    currentSteerAngle = maxSteerAngle * horizontalInput;

    Collider_FL.steerAngle = currentSteerAngle;
    Collider_FR.steerAngle = currentSteerAngle;

    float targetSteeringWheelAngle = -currentSteerAngle * steeringWheelTurnMultiplier;

    steeringWheelAngle = Mathf.Lerp(
        steeringWheelAngle,
        targetSteeringWheelAngle,
        Time.fixedDeltaTime * steeringWheelSmoothSpeed
    );

    steeringWheel.localRotation = Quaternion.AngleAxis(
        steeringWheelAngle,
        Vector3.forward
    );
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(Collider_FL, frontLeftWheelTransform);
        UpdateSingleWheel(Collider_FR, frontRightWheelTransform);
        UpdateSingleWheel(Collider_RL, rearLeftWheelTransform);
        UpdateSingleWheel(Collider_RR, rearRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
}