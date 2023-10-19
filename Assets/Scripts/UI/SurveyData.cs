using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class SurveyData : ScriptableObject
{
    [Header("Updated automatically")]
    public int surveyCount;
}
