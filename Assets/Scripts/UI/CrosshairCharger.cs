using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairCharger : MonoBehaviour
{
    [SerializeField] private Image[] fillImages;
    [SerializeField] private GameObject center;

    [Header("Charging Color Range"), SerializeField] private Color startColor, endColor;
    
    public static CrosshairCharger Instance { get; private set; }
    
    private void Start()
    {
        Instance = this;
        center.GetComponent<Image>().color = endColor;
        center.SetActive(false);
        foreach (var img in fillImages)
        {
            img.fillAmount = 0f;
        }
    }

    public void UpdateVisuals(float chargePercentage, bool overcharged = false)
    {
        chargePercentage = Mathf.Clamp01(chargePercentage);
        
        var chargeColor = overcharged ? Color.red : Color.Lerp(startColor, endColor, chargePercentage);

        foreach (var img in fillImages)
        {
            img.fillAmount = chargePercentage;
            img.color = chargeColor;
        }
        
        center.SetActive(chargePercentage >= 1f && !overcharged);
    }

    public void SetOverheat(bool on)
    {
        foreach (var img in fillImages)
        {
            img.fillAmount = on ? 1f : 0f;
            img.color = on ? Color.red : startColor;
        }
    }
}
