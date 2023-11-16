using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SurveyManager : MonoBehaviour
{
    [SerializeField] private SurveyData data;
    [SerializeField] private SurveySlider[] sliders;
    [SerializeField] private GameObject pointer;
    public static event Action OnSubmitSurvey;
    public static bool ShowingSurvey;

    public static string textFilePath { get; private set; }
    private string csvFilePath;

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
        ShowingSurvey = true;
        foreach (var slider in sliders)
        {
            slider.Init();
        }
        if (pointer != null)
            pointer.SetActive(true);
    }

    public void SubmitResults()
    {
        // Write Text file results
        StreamWriter writer = File.AppendText(textFilePath);

        // Optionally log boss fight time
        float bossFightTime = LevelManager.ReadBossFightTime();
        if (bossFightTime > 0f) // valid data read
        {
            writer.WriteLine("\nAFTER " + bossFightTime + " s IN BOSS FIGHT...");
        }
        
        writer.WriteLine("\nITERATION #" + iterationCounter++);
        
        foreach (var slider in sliders)
        {
            slider.Save();
            writer.WriteLine(slider.name + ": " + RatingToText(slider.previousValue));
        }
        writer.Close();
        
        // Write csv file results
        writer = File.AppendText(csvFilePath);
        
        // Optionally log boss fight time
        if (bossFightTime > 0f)
            writer.WriteLine("Boss Fight Duration;" + bossFightTime + ";;;");
        
        string line = "" + iterationCounter;
        
        foreach (var slider in sliders)
        {
            slider.Save();
            line += ";" + slider.previousValue;
        }
        writer.WriteLine(line);
        writer.Close();
        
        if (pointer != null)
            pointer.SetActive(false);
        
        gameObject.SetActive(false);
        OnSubmitSurvey?.Invoke();
        ShowingSurvey = false;
    }

    private string RatingToText(int rating)
    {
        switch (rating)
        {
            case 0: return "None";
            case 1: return "Low";
            case 2: return "Medium";
            case 3: return "High";
            case 4: return "Very High";
            default: return rating.ToString();
        }
    }
    
    // For testing without VR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            SubmitResults();
    }
}
