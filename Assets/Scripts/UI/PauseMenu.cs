using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Valve.VR;

public class PauseMenu : MonoBehaviour, InputActions.IPauseMenuActions
{
    private InputActions input;
    private GameObject panel;
    private bool paused;
    
    public SteamVR_Action_Boolean actionToggle = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "TogglePause");

    private void OnEnable()
    {
        if (input == null)
        {
            input = new InputActions();
            input.PauseMenu.SetCallbacks(this);
        }
        input.PauseMenu.Enable();

        actionToggle.onChange += Toggle;
    }

    private void OnDisable()
    {
        actionToggle.onChange -= Toggle;
    }

    // Start is called before the first frame update
    void Start()
    {
        panel = transform.GetChild(0).gameObject;
        Time.timeScale = 0f;
        paused = true;
    }

    private void Toggle(SteamVR_Action_Boolean action, SteamVR_Input_Sources sources, bool value)
    {
        paused = value;
        Time.timeScale = value ? 0f : 1f;
        panel.SetActive(value);
    }

    public void OnToggle(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (paused)
            {
                paused = false;
                Time.timeScale = 1f;
                panel.SetActive(false);
            }
            else
            {
                paused = true;
                Time.timeScale = 0f;
                panel.SetActive(true);
            }
        }
    }
}
