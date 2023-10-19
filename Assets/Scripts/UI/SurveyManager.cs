using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SurveyManager : MonoBehaviour
{
    [SerializeField] private SurveyData data;
    [SerializeField] private SurveySlider[] sliders;

    private string textFilePath, csvFilePath;

    private int iterationCounter = 0;
    
    private void Start()
    {
        // Create new text file for results
        textFilePath = Application.dataPath + "/SurveyResults/survey" + data.surveyCount + ".txt";
        StreamWriter writer = File.CreateText(textFilePath);
        writer.WriteLine("SURVEY RESULTS #" + data.surveyCount + "\t \tTimestamp: " + DateTime.Now);
        writer.Close();
        
        // Do the same for a .csv file
        csvFilePath = Application.dataPath + "/SurveyResults/survey" + data.surveyCount + ".csv";
        writer = File.CreateText(csvFilePath);
        
        // Header
        writer.WriteLine("Survey No.;Timestamp;;;");
        writer.WriteLine(data.surveyCount + ";" + DateTime.Now + ";;;");
        
        // Descriptions
        string descriptions = "Iteration";
        foreach (var slider in sliders)
        {
            descriptions += ";" + slider.name;
        }
        writer.WriteLine(descriptions);
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
        // Write Text file results
        StreamWriter writer = File.AppendText(textFilePath);
        writer.WriteLine("\nITERATION #" + iterationCounter++);
        
        foreach (var slider in sliders)
        {
            slider.Save();
            writer.WriteLine(slider.name + ": " + slider.previousValue);
        }
        writer.Close();
        
        // Write csv file results
        writer = File.AppendText(csvFilePath);
        string line = "" + iterationCounter;
        
        foreach (var slider in sliders)
        {
            slider.Save();
            line += ";" + slider.previousValue;
        }
        writer.WriteLine(line);
        writer.Close();
        
        gameObject.SetActive(false);
    }
}
