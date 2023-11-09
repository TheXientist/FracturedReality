using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Responsible for switching between boss fight and sanctuary "levels"
/// </summary>
public class LevelManager : MonoBehaviour
{
    private SurveyManager surveyUI;
    public static LevelManager instance;
    private RelaxationRoom relaxationRoom;
    [SerializeField] private BossAI boss;
    
    private enum SwitchCondition
    {
        BossDefeated, TimeOver, ButtonPress
    }

    [SerializeField] private SwitchCondition condition;

    [SerializeField] private float timeInRelaxationRoom;
    [SerializeField, Header("Only used if condition == TimeOver")] private float timeInBossFight;

    public float RemainingTime { get { return timeInBossFight - timer; } }
    
    private float timer;
    private bool isInRelaxationRoom;
    private bool firstSurveyAnswered, bothSurveysAnswered;

    private void Start()
    {
        instance = this;
        timer = 0f;

        relaxationRoom = FindObjectOfType<RelaxationRoom>();
        boss.OnDefeated += SwitchToRelaxationRoom;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        if (isInRelaxationRoom)
        {
            if (bothSurveysAnswered || (condition.Equals(SwitchCondition.ButtonPress) && Input.GetKeyDown(KeyCode.Backspace)))
            {
                SwitchToBossFight();
            } else if (!SurveyManager.ShowingSurvey && firstSurveyAnswered && (timer >= timeInRelaxationRoom))
            {
                StartCoroutine(ShowSurveyAfterSeconds(0f));
            }
            return;
        }
        // If in Boss fight

        switch (condition)
        {
            // Check time (death condition is checked by callback)
            case SwitchCondition.TimeOver when timer >= timeInBossFight:
            case SwitchCondition.ButtonPress when Input.GetKeyDown(KeyCode.Backspace):
                boss.TakeDamage(100000); // death -> despawn -> automatic switch
                break;
        }
    }

    private IEnumerator ShowSurveyAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);

        FindObjectOfType<PauseMenu>().DisallowToggle();
        surveyUI.gameObject.SetActive(true);
    }

    private void OnSurveySubmitted()
    {
        if (firstSurveyAnswered)
            bothSurveysAnswered = true;
        else
        {
            timer = 0f;
            firstSurveyAnswered = true;
        }
        FindObjectOfType<PauseMenu>().AllowToggle();
    }

    private void SwitchToBossFight()
    {
        StartCoroutine(relaxationRoom.ActivateBossFightRoom());

        SurveyManager.OnSubmitSurvey -= OnSurveySubmitted;
        isInRelaxationRoom = false;
        timer = 0f;
    }

    private void SwitchToRelaxationRoom()
    {
        StartCoroutine(relaxationRoom.ActivateRelaxRoom());
        StartCoroutine(ShowSurveyAfterSeconds(5f));
        
        if (surveyUI == null)
            surveyUI = FindObjectsOfType<SurveyManager>(true)[0];

        SurveyManager.OnSubmitSurvey += OnSurveySubmitted;
        firstSurveyAnswered = false;
        bothSurveysAnswered = false;
        isInRelaxationRoom = true;
    }
}
