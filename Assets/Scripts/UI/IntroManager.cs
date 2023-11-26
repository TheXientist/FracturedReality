using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    public int currentStep;
    [SerializeField] private GameObject pointer;

    [SerializeField] private bool skipIntro;

    [SerializeField, Header("Step 0")] private GameObject introText;
    [SerializeField] private GameObject header;
    private Image background;

    [SerializeField, Header("Step 1 (Survey)")]
    private GameObject survey;
    [SerializeField] private GameObject nextBtn;
    private TextMeshProUGUI btnText;
    
    [SerializeField, Header("Step 2")] private GameObject tutorialText0;
    [SerializeField] private GameObject spaceshipModel, repositionPanel;
    
    [SerializeField, Header("Step 3")] private SpaceshipController spaceshipController;
    [SerializeField] private GameObject tutorialText1;

    [SerializeField, Header("Step 4")] private GameObject tutorialText2;
    [SerializeField] private CanvasGroup hud;

    [SerializeField, Header("Step 5")] private GameObject tutorialText3;
    
    // ...

    [SerializeField, Header("Step 6")] private Player playerControls;

    [SerializeField, Header("Step 7")] private GameObject tutorialText4;

    [SerializeField, Header("Step 8")] private GameObject boss;
    [SerializeField] private GameObject bossAnimator, arena, dummyFractal, dummyTransforms;

    private void Awake()
    {
        background = GetComponent<Image>();
        btnText = nextBtn.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        pointer.SetActive(true);
    }

    public void OnNextButton()
    {
        // Somehow this gets 1 call with the first RB after intro was closed
        if (!gameObject.activeSelf) return;
        
        switch (currentStep)
        {
            case 0:
                if (skipIntro)
                {
                    // Activate relevant graphics and skip to last step
                    spaceshipModel.SetActive(true);
                    hud.alpha = 1f;
                    goto case 9;
                }
                // Disable everything, enable survey
                background.enabled = false;
                header.SetActive(false);
                introText.SetActive(false);
                nextBtn.SetActive(false);
                survey.SetActive(true);
                SurveyManager.OnSubmitSurvey += OnSurveySubmitted;
                break;
            case 1:
                // Turn everything back on, show reposition text
                pointer.SetActive(true);
                background.enabled = true;
                header.SetActive(true);
                tutorialText0.SetActive(true);
                nextBtn.SetActive(true);
                spaceshipModel.SetActive(true);
                btnText.text = "Reposition Menu";
                break;
            case 2:
                // Show reposition panel
                background.enabled = false;
                header.SetActive(false);
                tutorialText0.SetActive(false);
                repositionPanel.SetActive(true);
                repositionPanel.GetComponent<RepositionPlayer>()?.closeButton.SetActive(false);
                btnText.text = "Done";
                break;
            case 3:
                // Show tutorial pt. 1
                Time.timeScale = 1f; // is left at 0 by reposition panel
                repositionPanel.GetComponent<RepositionPlayer>()?.closeButton.SetActive(true);
                repositionPanel.SetActive(false);
                background.enabled = true;
                header.SetActive(true);
                tutorialText0.SetActive(false);
                tutorialText1.SetActive(true);
                btnText.text = "Test";
                break;
            case 4:
                // Only show button, enable controls
                background.enabled = false;
                header.SetActive(false);
                tutorialText1.SetActive(false);
                spaceshipController.enabled = true;
                btnText.text = "Continue";
                break;
            case 5:
                // Show tutorial pt. 2
                background.enabled = true;
                header.SetActive(true);
                spaceshipController.enabled = false;
                tutorialText2.SetActive(true);
                hud.alpha = 1f;
                btnText.text = "Next";
                break;
            case 6:
                // Show tutorial pt. 3
                tutorialText2.SetActive(false);
                tutorialText3.SetActive(true);
                btnText.text = "Test";
                break;
            case 7:
                // Only show button, enable controls
                background.enabled = false;
                header.SetActive(false);
                tutorialText3.SetActive(false);
                spaceshipController.enabled = true;
                playerControls.enabled = true;
                btnText.text = "Continue";
                break;
            case 8:
                // Show tutorial pt. 3
                background.enabled = true;
                header.SetActive(true);
                spaceshipController.enabled = false;
                playerControls.enabled = false;
                tutorialText4.SetActive(true);
                btnText.text = "Start";
                break;
            case 9:
                // Begin fight
                spaceshipController.enabled = true;
                playerControls.enabled = true;
                dummyFractal.SetActive(false);
                dummyFractal.GetComponent<Collider>().enabled = false; // not needed anymore
                dummyTransforms.SetActive(false);
                boss.SetActive(true);
                bossAnimator.SetActive(true);
                SetBossShaderParameters();
                arena.SetActive(true);
                
                // Start boss fight timer
                FindObjectOfType<LevelManager>().RestartTimer();
                
                pointer.SetActive(false);
                FindObjectOfType<PauseMenu>().AllowToggle();
                gameObject.SetActive(false);
                break;
        }

        currentStep++;
    }

    public void OnSurveySubmitted()
    {
        if (currentStep != 1) return;

        SurveyManager.OnSubmitSurvey -= OnSurveySubmitted;
        OnNextButton();
    }
    
    // For testing without VR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && currentStep != 1)
            OnNextButton();
    }

    private void SetBossShaderParameters()
    {
        Material material = GameObject.FindWithTag("RenderPlane").GetComponent<MeshRenderer>().material;
        material.SetFloat("_AO", 0.8f);
        material.SetFloat("_IAmbient", 0.7f);
        material.SetFloat("_LightExposure", 5.7f);
        material.SetFloat("_LightIntensity", 6f);
        material.SetFloat("_SaturationGamma", 2f);
        material.SetInteger("_LightMode", 0);
    }
}
