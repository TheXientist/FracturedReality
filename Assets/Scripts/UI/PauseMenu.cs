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
    private bool started;
    
    public SteamVR_Action_Boolean actionToggle = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "TogglePause");

    private void OnEnable()
    {
        if (input == null)
        {
            input = new InputActions();
            input.PauseMenu.SetCallbacks(this);
        }
        input.PauseMenu.Enable();

        actionToggle.onChange += ToggleVR;
    }

    private void OnDisable()
    {
        actionToggle.onChange -= ToggleVR;
    }

    // Start is called before the first frame update
    void Start()
    {
        panel = transform.GetChild(0).gameObject;

        StartCoroutine(PauseAfterSeconds(1f));
    }

    private IEnumerator PauseAfterSeconds(float s)
    {
        yield return new WaitForSeconds(s);
        Toggle(true);
    }

    private void ToggleVR(SteamVR_Action_Boolean action, SteamVR_Input_Sources sources, bool value)
    {
        Toggle(!value);
    }

    private void Toggle(bool shouldPause)
    {
        paused = shouldPause;

        if(panel != null)
        {
            panel.SetActive(shouldPause);
        }
        
        Time.timeScale = paused ? 0f : 1f;
    }

    public void OnToggle(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Toggle(!paused);
        }
    }

    public IEnumerator PauseOnReload(float s)
    {
        yield return new WaitForSeconds(s);
        Toggle(true);
    }

    private void Update()
    {
        if(paused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

}
