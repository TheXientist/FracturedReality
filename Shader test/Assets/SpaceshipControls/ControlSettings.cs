using System;
using UnityEngine;

[CreateAssetMenu]
public class ControlSettings : ScriptableObject
{
    public enum DOF
    {
        moveX = 1, moveY = 2, moveZ = 4,
        rotateX = 8, rotateY = 16, rotateZ = 32
    }
    
    [Header("Left Stick Input Mapping")] 
    public DOF leftStickX;
    public bool invertLX;
    public DOF leftStickY;
    public bool invertLY;

    [Header("Right Stick Input Mapping")]
    public DOF rightStickX;
    public bool invertRX;
    public DOF rightStickY;
    public bool invertRY;

    [Header("Additional Input Parameters")]
    [Range(0f, 1f)] public float deadzonePerAxis;

    [Header("Movement Parameters")]
    public float moveSpeed;
    public float moveAcceleration, moveDeceleration;
    public float rotationSpeed, rotationAcceleration, rotationDeceleration;

    public void SolveInput(ref Vector3 moveInput, ref Vector3 rotationInput, Vector2 lStick, Vector2 rStick)
    {
        // Map all the stick values into their corresponding input axis
        SolveAxis(ref moveInput, ref rotationInput, leftStickX, invertLX ? -lStick.x : lStick.x);
        SolveAxis(ref moveInput, ref rotationInput, leftStickY, invertLY ? -lStick.y : lStick.y);
        SolveAxis(ref moveInput, ref rotationInput, rightStickX, invertRX ? -rStick.x : rStick.x);
        SolveAxis(ref moveInput, ref rotationInput, rightStickY, invertRY ? -rStick.y : rStick.y);
    }
    
    private void SolveAxis(ref Vector3 moveInput, ref Vector3 rotationInput, DOF map, float input)
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
    
    /// <summary>
    /// Applies the deadzone (set in inspector) to each input axis separately
    /// </summary>
    public void ApplyDeadzone(ref Vector2 vector)
    {
        vector.x = vector.x >= 0f ? Mathf.InverseLerp(deadzonePerAxis, 1f, vector.x) : -Mathf.InverseLerp(deadzonePerAxis, 1f, -vector.x);
        vector.y = vector.y >= 0f ? Mathf.InverseLerp(deadzonePerAxis, 1f, vector.y) : -Mathf.InverseLerp(deadzonePerAxis, 1f, -vector.y);
    }
}
