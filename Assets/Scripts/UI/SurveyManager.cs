using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SurveyManager : MonoBehaviour
{
    [SerializeField] private SurveyData data;
    [SerializeField] private SurveySlider[] sliders;

    private string resultsFilePath;

    private int iterationCounter = 0;
    
    private void Start()
    {
        // Create new text file for results
        resultsFilePath = Application.dataPath + "/SurveyResults/survey" + data.surveyCount + ".txt";
        StreamWriter writer = File.CreateText(resultsFilePath);
        writer.WriteLine("SURVEY RESULTS #" + data.surveyCount + "\t \tTimestamp: " + DateTime.Now);
        writer.Close();
        data.surveyCount++;
    }

    public void OnEnable()
    {
        foreach (var slider in sliders)
        {
            slider.Init();
        }
    }

    public void SubmitResults()
    {
        StreamWriter writer = File.AppendText(resultsFilePath);
        writer.WriteLine("\nITERATION #" + iterationCounter++);
        
        foreach (var slider in sliders)
        {
            slider.Save();
            writer.WriteLine(slider.name + ": " + slider.previousValue);
        }
        writer.Close();
        
        gameObject.SetActive(false);
    }
}
