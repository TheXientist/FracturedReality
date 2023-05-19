using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Valve.VR;
using DOF = ControlSettings.DOF;

public class SpaceshipController : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefabVR, camera;
    [SerializeField] private bool debug, vr;
    private SteamVR_Action_Vector2 leftStickAction, rightStickAction;
    private GameObject vrArmature;

    private Vector2 leftStickInput, rightStickInput;

    [SerializeField] private ControlSettings defaultControls, lockedOnControls;
    private ControlSettings currentControls;

    private Vector3 velocity, angularVelocity;
    
    private Vector3 moveInput, rotationInput;
    
    private Rigidbody rb;
    
    [SerializeField] private Transform target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (vr)
        {
            InitVR();
        }
        
        SetControlSettings(target ? lockedOnControls : defaultControls);
    }

    private void SetControlSettings(ControlSettings controls)
    {
        currentControls = controls;
        FreezeUnusedRotations();
    }

    private void FreezeUnusedRotations()
    {
        rb.constraints = RigidbodyConstraints.None;
        DOF usedAxes = currentControls.leftStickX | currentControls.leftStickY | currentControls.rightStickX | currentControls.rightStickY;

        if ((usedAxes & DOF.rotateX) == 0) rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        if ((usedAxes & DOF.rotateY) == 0) rb.constraints |= RigidbodyConstraints.FreezeRotationY;
        if ((usedAxes & DOF.rotateZ) == 0) rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
    }

    private void InitVR()
    {
        vrArmature = Instantiate(playerPrefabVR, transform);
        vrArmature.SetActive(true);
        camera.SetActive(false);
        
        SteamVR_Actions._default.Deactivate();
        SteamVR_Actions.spaceship.Activate();
        
        leftStickAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("spaceship", "leftStick");
        rightStickAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("spaceship", "rightStick");
    }

    private void Update()
    {
        ParseInputs();
    }

    private void FixedUpdate()
    {
        RotationUpdate();
        MoveUpdate();
    }

    private void ParseInputs()
    {
        // Overwrite input system w/ VR actions
        if (vr)
        {
            leftStickInput = leftStickAction.axis;
            rightStickInput = rightStickAction.axis;
            currentControls.ApplyDeadzone(ref leftStickInput);
            currentControls.ApplyDeadzone(ref rightStickInput);
        }

        // Reset all inputs
        moveInput = Vector3.zero;
        rotationInput = Vector3.zero;

        // Let the control settings do the mapping
        currentControls.SolveInput(ref moveInput, ref rotationInput, leftStickInput, rightStickInput);

        if (!debug) return;
        Debug.Log("LeftStick: " + leftStickInput);
        Debug.Log("RightStick: " + rightStickInput);
        Debug.Log("MoveInput: " + moveInput);
        Debug.Log("RotationInput: " + rotationInput);
        Debug.Log("Velocity: " + velocity);
        Debug.Log("AngularVelocity: " + angularVelocity);
    }

    private void OnLeftStick(InputValue value)
    {
        leftStickInput = value.Get<Vector2>();
        
        currentControls.ApplyDeadzone(ref leftStickInput);
    }

    private void OnRightStick(InputValue value)
    {
        rightStickInput = value.Get<Vector2>();
        
        currentControls.ApplyDeadzone(ref rightStickInput);
    }

    private void MoveUpdate()
    {
        
        
        velocity += moveInput * currentControls.moveAcceleration;
        
        
        // Simulate gravity (doesn't make sense but feels better)
        
        if (moveInput.x == 0f)
            if (Mathf.Abs(velocity.x) > currentControls.moveDeceleration)
                velocity.x += currentControls.moveDeceleration * (velocity.x > 0f ? -1f : 1f);
            else
                velocity.x = 0f;
        if (moveInput.y == 0f)
            if (Mathf.Abs(velocity.y) > currentControls.moveDeceleration)
                velocity.y += currentControls.moveDeceleration * (velocity.y > 0f ? -1f : 1f);
            else
                velocity.y = 0f;
        if (moveInput.z == 0f)
            if (Mathf.Abs(velocity.z) > currentControls.moveDeceleration)
                velocity.z += currentControls.moveDeceleration * (velocity.z > 0f ? -1f : 1f);
            else
                velocity.z = 0f;
        
        
        velocity = Vector3Extensions.Clamp(velocity, -currentControls.moveSpeed * Vector3.one, currentControls.moveSpeed * Vector3.one);

        Vector3 movementVector = velocity.z * transform.forward;
        movementVector += velocity.x * transform.right;
        movementVector += velocity.y * transform.up;

        rb.velocity = movementVector;
    }

    private void RotationUpdate()
    {
        if (target)
        {
            rb.angularVelocity = Vector3.zero;
            // Calculate the rotation angle to look at the target object
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, transform.up);
            Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, currentControls.rotationSpeed);
            rb.MoveRotation(newRotation);
            return;
        }
        
        angularVelocity += rotationInput * currentControls.rotationAcceleration;
        
        // Stop rotating if there's no input
        
        if (rotationInput.x == 0f)
            if (Mathf.Abs(angularVelocity.x) > currentControls.rotationDeceleration)
                angularVelocity.x += currentControls.rotationDeceleration * (angularVelocity.x > 0f ? -1f : 1f);
            else
                angularVelocity.x = 0f;
        if (rotationInput.y == 0f)
            if (Mathf.Abs(angularVelocity.y) > currentControls.rotationDeceleration)
                angularVelocity.y += currentControls.rotationDeceleration * (angularVelocity.y > 0f ? -1f : 1f);
            else
                angularVelocity.y = 0f;
        if (rotationInput.z == 0f)
            if (Mathf.Abs(angularVelocity.z) > currentControls.rotationDeceleration)
                angularVelocity.z += currentControls.rotationDeceleration * (angularVelocity.z > 0f ? -1f : 1f);
            else
                angularVelocity.z = 0f;

        angularVelocity =
            Vector3Extensions.Clamp(angularVelocity, -currentControls.rotationSpeed * Vector3.one, currentControls.rotationSpeed * Vector3.one);

        Vector3 rotationVector = angularVelocity.z * transform.forward;
        rotationVector += angularVelocity.x * transform.right;
        rotationVector += angularVelocity.y * transform.up;
        rb.angularVelocity = rotationVector;
    }
    
    public void SetTarget(Transform t)
    {
        // TODO: test if setting is legal
        target = t;
        SetControlSettings(lockedOnControls);
    }

    public void RemoveTarget()
    {
        target = null;
        SetControlSettings(defaultControls);
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
