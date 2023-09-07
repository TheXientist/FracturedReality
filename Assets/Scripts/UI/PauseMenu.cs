using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Valve.VR;

public class PauseMenu : MonoBehaviour, InputActions.IPauseMenuActions
{
    [SerializeField] private AudioClip menuTheme;
    [SerializeField] private GameObject UIPointer;
    [SerializeField] private GameObject m_repositionPanel;
    private InputActions input;
    private GameObject panel;
    private bool paused;
    private bool started;
    private bool allowToggle = true;
    private bool neverClosedYet = true;
    
    public SteamVR_Action_Boolean actionToggle = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "TogglePause");

    private void OnEnable()
    {
        if (input == null)
        {
            input = new InputActions();
            input.PauseMenu.SetCallbacks(this);
        }
        input.PauseMenu.Enable();

        actionToggle.AddOnStateDownListener(ToggleVR, SteamVR_Input_Sources.Any);
    }

    private void OnDisable()
    {
        actionToggle.RemoveOnStateDownListener(ToggleVR, SteamVR_Input_Sources.Any);
    }

    // Start is called before the first frame update
    void Start()
    {
        panel = transform.GetChild(0).gameObject;

        StartCoroutine(PauseAfterSeconds(1f));
        Player.OnPlayerDeath += DisallowToggle;
    }

    private IEnumerator PauseAfterSeconds(float s)
    {
        yield return new WaitForSeconds(s);
        Toggle(true);
    }

    private void ToggleVR(SteamVR_Action_Boolean action, SteamVR_Input_Sources sources)
    {       
        Toggle(!paused);
    }

    private void Toggle(bool shouldPause)
    {
        if (!allowToggle) return;

        paused = shouldPause;

        if(panel != null)
        {
            panel.SetActive(shouldPause);

            if (SpaceshipController.VR)
            {
                UIPointer.SetActive(shouldPause);
                m_repositionPanel.SetActive(false);
            }

        }
        
        Time.timeScale = paused ? 0f : 1f;

        if (!paused && neverClosedYet)
        {
            StartCoroutine(FindObjectOfType<MusicFader>().FadeOut(0.5f));
            neverClosedYet = false;
            BossAI.Instance.gameObject.SetActive(true);
        }
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
        neverClosedYet = true;
        Toggle(true);
    }

    private void DisallowToggle()
    {
        allowToggle = false;
    }

    public void AllowToggle()
    {
        allowToggle = true;
    }

        private void Update()
    {
     /* if(paused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        } */
    }

}
