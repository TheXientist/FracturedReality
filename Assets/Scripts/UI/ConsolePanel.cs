using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Valve.VR;

public class ConsolePanel : MonoBehaviour
{
    private Dictionary<string, string> staticLogs = new Dictionary<string, string>();
    private Queue<string> dynamicLogs = new Queue<string>();

    public GameObject panel;
    public Text staticDisplay, scrollingDisplay;

    public uint maxDynamicLogs;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void Update()
    {
        // Test
        //Debug.Log("Time: " + Time.time);
        //Debug.Log(Time.deltaTime);
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Log)
        {
            string[] splitString = logString.Split(char.Parse(":"));
            string debugKey = splitString[0];

            if (splitString.Length > 1)
            {
                string debugValue = splitString[1];

                if (staticLogs.ContainsKey(debugKey))
                    staticLogs[debugKey] = debugValue;
                else
                {
                    staticLogs.Add(debugKey, debugValue);
                }
            }
            else
            {
                if (dynamicLogs.Count == maxDynamicLogs)
                    dynamicLogs.Dequeue();
                
                dynamicLogs.Enqueue(debugKey);
            }
            
        }

        // Static
        
        string displayText = "";
        foreach (var log in staticLogs)
        {
            displayText += log.Key + ": " + log.Value + "\n";
        }

        staticDisplay.text = displayText;

        displayText = "";
        foreach (var log in dynamicLogs)
        {
            displayText += log + "\n";
        }

        scrollingDisplay.text = displayText;
    }
}
