using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SC_SpaceshipController : MonoBehaviour
{
    public enum DOF
    {
        moveX, moveY, moveZ,
        rotateX, rotateY, rotateZ
    }
    
    [SerializeField] private bool vr;
    private SteamVR_Action_Vector2 leftStickAction;
    private SteamVR_Action_Vector2 rightStickAction;

    //public Interactable leftController, rightController;

    private Vector2 leftStickInput, rightStickInput;

    [SerializeField, Header("Left Stick Input Mapping")] 
    private DOF leftStickX;
    [SerializeField] private DOF leftStickY;

    [SerializeField, Header("Right Stick Input Mapping")]
    private DOF rightStickX;
    [SerializeField] private DOF rightStickY;

    [Header("Movement Parameters"), SerializeField]
    private float moveSpeed;
    [SerializeField] private float moveAcceleration, moveDeceleration;
    [SerializeField] private float rotationSpeed, rotationAcceleration, rotationDeceleration;

    private Vector3 velocity, angularVelocity;
    
    private Vector3 moveInput, rotationInput;
    
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (vr)
        {
            InitVR();
        }
    }

    private void InitVR()
    {
        leftStickAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("spaceship", "move");
        rightStickAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("spaceship", "rotate");
    }

    private void Update()
    {
        ParseInputs();
    }

    private void FixedUpdate()
    {
        MoveUpdate();
        RotationUpdate();
    }

    private void ParseInputs()
    {
        // Overwrite input system w/ VR actions
        if (vr)
        {
            leftStickInput = leftStickAction.axis;
            rightStickInput = rightStickAction.axis;
        }
        
        // Reset all inputs
        moveInput = Vector3.zero;
        rotationInput = Vector3.zero;

        // Map all the stick values into their corresponding input axis
        SolveMapping(leftStickX, leftStickInput.x);
        SolveMapping(leftStickY, leftStickInput.y);
        SolveMapping(rightStickX, rightStickInput.x);
        SolveMapping(rightStickY, rightStickInput.y);

        Debug.Log("LeftStick: " + leftStickInput);
        Debug.Log("RightStick: " + rightStickInput);
        Debug.Log("MoveInput: " + moveInput);
        Debug.Log("RotationInput: " + rotationInput);
        Debug.Log("Velocity: " + velocity);
    }

    private void SolveMapping(DOF map, float input)
    {
        switch (map)
        {
            case DOF.moveX:
                moveInput.x = input;
                break;
            case DOF.moveY:
                moveInput.y = input;
                break;
            case DOF.moveZ:
                moveInput.z = input;
                break;
            case DOF.rotateX:
                rotationInput.x = input;
                break;
            case DOF.rotateY:
                rotationInput.y = input;
                break;
            case DOF.rotateZ:
                rotationInput.z = input;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(map), map, null);
        }
    }

    private void OnLeftStick(InputValue value)
    {
        leftStickInput = value.Get<Vector2>();
    }

    private void OnRightStick(InputValue value)
    {
        rightStickInput = value.Get<Vector2>();
    }

    private void MoveUpdate()
    {
        
        
        velocity += moveInput * moveAcceleration;
        
        
        // Simulate gravity (doesn't make sense but feels better)
        
        if (moveInput.x == 0f)
            if (Mathf.Abs(velocity.x) > moveDeceleration)
                velocity.x += moveDeceleration * (velocity.x > 0f ? -1f : 1f);
            else
                velocity.x = 0f;
        if (moveInput.y == 0f)
            if (Mathf.Abs(velocity.y) > moveDeceleration)
                velocity.y += moveDeceleration * (velocity.y > 0f ? -1f : 1f);
            else
                velocity.y = 0f;
        if (moveInput.z == 0f)
            if (Mathf.Abs(velocity.z) > moveDeceleration)
                velocity.z += moveDeceleration * (velocity.z > 0f ? -1f : 1f);
            else
                velocity.z = 0f;
        
        
        velocity = Vector3Extensions.Clamp(velocity, -moveSpeed * Vector3.one, moveSpeed * Vector3.one);

        Vector3 movementVector = velocity.z * transform.forward;
        movementVector += velocity.x * transform.right;
        movementVector += velocity.y * transform.up;

        rb.velocity = movementVector;
    }

    private void RotationUpdate()
    {
        /*
        Vector3 currentRotationEulers = rigidbody.rotation.eulerAngles;
        Vector3 targetRotationEulers = currentRotationEulers + rotationInput;
        rigidbody.MoveRotation(Quaternion.Euler(targetRotationEulers));*/
        
        
    }
}

public static class Vector3Extensions
{
    public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
    {
        float x = Mathf.Clamp(vector.x, min.x, max.x);
        float y = Mathf.Clamp(vector.y, min.y, max.y);
        float z = Mathf.Clamp(vector.z, min.z, max.z);
        return new Vector3(x, y, z);
    }
}
