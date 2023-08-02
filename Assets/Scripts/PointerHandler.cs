using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using Valve.VR;


public class PointerHandler : MonoBehaviour
{
    public SteamVR_Action_Boolean inputAction;

    RaycastHit[] hits;

    private void Start()
    {
        //inputAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("spaceship", "fire");
        inputAction.AddOnChangeListener(UIInteraction, SteamVR_Input_Sources.Any);
    }

    public void UIInteraction(SteamVR_Action_Boolean action, SteamVR_Input_Sources source, bool value) => UIInteractionButtonPressed(value);

    private void UIInteractionButtonPressed(bool btnDown)
    {
        if (btnDown)
        {

            foreach (RaycastHit hit in hits)
            {

                if (hit.collider.gameObject.GetComponent<Button>() != null)
                {

                    hit.collider.gameObject.GetComponent<Button>().onClick.Invoke();
                    hit.collider.gameObject.GetComponent<VRButton>().ResetPressedTimer();
                    string hitObjectName = hit.collider.gameObject.name;
                    print(hitObjectName);
                }

            }
        }
 
    }

    void Update()
    {       
        //Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        Debug.DrawRay(transform.position, transform.forward, Color.green);

        
        Ray ray = new Ray(transform.position, transform.forward);
        hits = Physics.RaycastAll(ray,200f);

        foreach (RaycastHit hit in hits)
        {

            if (hit.collider.gameObject.GetComponent<VRButton>() != null)
            {

                hit.collider.gameObject.GetComponent<VRButton>().isHovering();
            }

        }


    }

}
