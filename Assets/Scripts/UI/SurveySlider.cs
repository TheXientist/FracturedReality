using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurveySlider : MonoBehaviour
{
    public int previousValue { get; private set; } = -1;

    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform sliderHandle;
    [SerializeField] private RectTransform previousMarker;

    public void Init()
    {
        if (previousValue == -1)
        {
            slider.value = 2f;
            previousMarker.gameObject.SetActive(false);
            return;
        }

        slider.value = previousValue;
        previousMarker.gameObject.SetActive(true);
        previousMarker.position = sliderHandle.position;
        previousMarker.anchoredPosition += 40f * Vector2.up;
    }

    public void Increase()
    {
        if (slider.value < 4f)
            slider.value++;
    }

    public void Decrease()
    {
        if (slider.value > 0f)
            slider.value--;
    }

    public void Save()
    {
        previousValue = (int)slider.value;
    }
}
