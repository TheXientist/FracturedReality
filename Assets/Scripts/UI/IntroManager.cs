using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    public int step;

    private Image background;
    [SerializeField] private GameObject introText, header, survey;
    [SerializeField] private GameObject nextBtn;
    [SerializeField] private GameObject tutorialText1;
    
    public void OnNextButton()
    {
        switch (step)
        {
            case 0:
                // Disable everything, enable survey
                background.enabled = false;
                header.SetActive(false);
                introText.SetActive(false);
                nextBtn.SetActive(false);
                survey.SetActive(true);
                break;
            case 1:
                // Turn everything back on, show tutorial 1
                background.enabled = true;
                header.SetActive(true);
                tutorialText1.SetActive(true);
                nextBtn.SetActive(true);
                break;
        }

        step++;
    }

    public void OnSurveySubmitted()
    {
        if (step != 1) return;

        OnNextButton();
    }
}
