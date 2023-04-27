using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SpaceshipController : MonoBehaviour
{
    public SteamVR_Action_Vector2 actionMove = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("spaceship", "move");
    public SteamVR_Action_Vector2 actionRotate = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("spaceship", "rotate");

    public Interactable leftController, rightController;

    private Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }


    private void Update()
    {
        Debug.Log("Action is active? " + actionMove.active + "\nMove Input: " + actionMove.axis + "\nRotation Input: " + actionRotate.axis);
    }

    private void MoveUpdate()
    {
        Vector2 input = actionMove.axis;
        rigidbody.AddForce(new Vector3(input.x, input.y, 0f));
    }

    private void RotationUpdate()
    {
        Vector2 input = actionRotate.axis;
        Vector3 currentRotationEulers = rigidbody.rotation.eulerAngles;
        Vector3 targetRotationEulers = currentRotationEulers + new Vector3(input.x, input.y, 0f);
        rigidbody.MoveRotation(Quaternion.Euler(targetRotationEulers));
    }
}
