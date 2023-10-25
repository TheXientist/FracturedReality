using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for switching between boss fight and sanctuary "levels"
/// </summary>
public class LevelManager : MonoBehaviour
{
    private RelaxationRoom relaxationRoom;
    [SerializeField] private BossAI boss;
    
    private enum SwitchCondition
    {
        BossDefeated, TimeOver
    }

    [SerializeField] private SwitchCondition condition;

    [SerializeField] private float timeInRelaxationRoom;
    [SerializeField, Header("Only used if condition == TimeOver")] private float timeInBossFight;

    private float timer;
    private bool isInRelaxationRoom;
    [SerializeField] private bool surveyAnswered;

    private void Start()
    {
        timer = 0f;
        
        relaxationRoom = FindObjectOfType<RelaxationRoom>();
        boss.OnDefeated += SwitchToRelaxationRoom;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        if (isInRelaxationRoom)
        {
            if (surveyAnswered && timer >= timeInRelaxationRoom)
            {
                SwitchToBossFight();
            }
            return;
        }
        // If in Boss fight
        
        // Check time (death condition is checked by callback)
        if (condition.Equals(SwitchCondition.TimeOver) && timer >= timeInBossFight)
            boss.TakeDamage(100000); // death -> despawn -> automatic switch
    }

    private void SwitchToBossFight()
    {
        StartCoroutine(relaxationRoom.ActivateBossFightRoom());

        isInRelaxationRoom = false;
        timer = 0f;
    }

    private void SwitchToRelaxationRoom()
    {
        StartCoroutine(relaxationRoom.ActivateRelaxRoom());

        isInRelaxationRoom = true;
        timer = 0f;
    }
}
