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
    
    [SerializeField, Header("Step 2")] private GameObject tutorialText1;
    [SerializeField] private GameObject spaceshipModel;
    [SerializeField] private SpaceshipController spaceshipController;

    [SerializeField, Header("Step 3")] private GameObject tutorialText2;
    
    // ...

    [SerializeField, Header("Step 4")] private Player playerControls;
    [SerializeField] private CanvasGroup hud;

    [SerializeField, Header("Step 5")] private GameObject tutorialText3;

    [SerializeField, Header("Step 6")] private GameObject boss;
    [SerializeField] private GameObject bossAnimator, obstacles, arena, dummyFractal, dummyTransforms;

    private void Awake()
    {
        background = GetComponent<Image>();
    }

    private void Start()
    {
        pointer.SetActive(true);
    }

    public void OnNextButton()
    {
        switch (currentStep)
        {
            case 0:
                if (skipIntro)
                {
                    // Activate relevant graphics and skip to last step
                    spaceshipModel.SetActive(true);
                    hud.alpha = 1f;
                    goto case 6;
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
                // Turn everything back on, show tutorial pt. 1
                background.enabled = true;
                header.SetActive(true);
                tutorialText1.SetActive(true);
                nextBtn.SetActive(true);
                spaceshipModel.SetActive(true);
                btnText.text = "Test";
                break;
            case 2:
                // Only show button, enable controls
                background.enabled = false;
                header.SetActive(false);
                tutorialText1.SetActive(false);
                spaceshipController.enabled = true;
                btnText.text = "Continue";
                break;
            case 3:
                // Show tutorial pt. 2
                background.enabled = true;
                header.SetActive(true);
                spaceshipController.enabled = false;
                tutorialText2.SetActive(true);
                hud.alpha = 1f;
                btnText.text = "Test";
                break;
            case 4:
                // Only show button, enable controls
                background.enabled = false;
                header.SetActive(false);
                tutorialText2.SetActive(false);
                spaceshipController.enabled = true;
                playerControls.enabled = true;
                btnText.text = "Continue";
                break;
            case 5:
                // Show tutorial pt. 3
                background.enabled = true;
                header.SetActive(true);
                spaceshipController.enabled = false;
                playerControls.enabled = false;
                tutorialText3.SetActive(true);
                btnText.text = "Start";
                break;
            case 6:
                // Begin fight
                spaceshipController.enabled = true;
                playerControls.enabled = true;
                dummyFractal.SetActive(false);
                dummyFractal.GetComponent<Collider>().enabled = false; // not needed anymore
                dummyTransforms.SetActive(false);
                boss.SetActive(true);
                bossAnimator.SetActive(true);
                SetBossShaderParameters();
                obstacles.SetActive(true);
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
