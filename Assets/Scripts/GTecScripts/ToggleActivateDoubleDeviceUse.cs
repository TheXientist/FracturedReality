using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleActivateDoubleDeviceUse : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown DoubleDropdown;
    public void ActivateDoubleUse(bool input)
    {
        if (input)
        {
            DoubleDropdown.interactable = true;
        }
        else
        {
            DoubleDropdown.interactable = false;
        }
    }
}
