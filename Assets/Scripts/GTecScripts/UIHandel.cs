using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.Serialization;

public class UIHandel : MonoBehaviour
{
    
    [SerializeField] private TMP_Text header;
    [SerializeField] private TMP_Text firstDeviceName;
    [SerializeField] private TMP_Dropdown firstDeviceDropdown;
    [SerializeField] private TMP_Text secondDeviceName;
    [SerializeField] private TMP_Dropdown secondDeviceDropdown;
    [SerializeField] private Button continueButton;
    [SerializeField] private WriteTime readerWriter;
    [SerializeField] private TMP_InputField firstInputField;
    [SerializeField] private TMP_InputField secondInputField;
    [SerializeField] public Toggle firstToggleDoubleUse;
    [SerializeField] public Toggle secondToggleDoubleUse;
    [SerializeField] private TMP_Dropdown firstDoubleUseDropdown;
    [SerializeField] private TMP_Dropdown secondDoubleUseDropdown;
    [SerializeField] private Button recordButton;
    [SerializeField] private TMP_Text recordButtonText;
    [SerializeField] private Button writeButton;
    //device Type 0 = None, 1 = EEG, 2 = GSR, 3 = Pulse, 4 = Breathing
    public int firstDeviceType;
    public int secondDeviceType;
    public int doubleDeviceType;

    private bool isOnlyDevice = false;

    [SerializeField] private SensorReader sensorReader;
    private void Start()
    {
        header.text = "Searching devices...";
        //MonoScript ms = MonoScript.FromMonoBehaviour(this);
        //string path = AssetDatabase.GetAssetPath(ms);
        //Debug.Log(path);
    }

    //Dropdown selections:
    // 0 - None
    // 1 - EEG
    // 2 - GSR
    // 3 - Pulse
    // 4 - Breathing
    public void OnFirstDropdownChange(int selection)
    {
        firstDeviceType = selection;
        if (isOnlyDevice)
        {
            if(firstDeviceType == 1)
                readerWriter.oneDevice = 1;
            else
            {
                readerWriter.oneDevice = 2;
            }
        }
    }

    public void OnDoubleDropdownChange(int selection)
    {
        doubleDeviceType = selection;
    }
    
    //Dropdown selections:
    // 0 - EEG
    // 1 - GSR
    // 2 - Pulse
    // 3 - Breathing
    public void OnSecondDropdownChange(int selection)
    {
        secondDeviceType = selection;
    }

    public void SetTexts(List<string> serialNumbers)
    {
        if (serialNumbers.Count == 1)
        {
            header.text = "Connected device:";
            firstDeviceName.text = serialNumbers[0];
            firstDeviceDropdown.interactable = true;
            firstInputField.interactable = true;
            continueButton.interactable = true;
            firstToggleDoubleUse.interactable = true;
            isOnlyDevice = true;
        }
        else if (serialNumbers.Count >= 2)
        {
            header.text = "Connected devices:";
            firstDeviceName.text = serialNumbers[0];
            secondDeviceName.text = serialNumbers[1];
            firstDeviceDropdown.interactable = true;
            secondDeviceDropdown.interactable = true;
            firstInputField.interactable = true;
            secondInputField.interactable = true;
            continueButton.interactable = true;
            secondToggleDoubleUse.interactable = true;
            readerWriter.oneDevice = 0;
        }
        else
        {
            header.text = "Error.";
        }
    }

    public void OnPressContinueButton()
    {
        sensorReader.ContinueDeviceSetup();
        continueButton.interactable = false;
        firstDeviceDropdown.interactable = false;
        secondDeviceDropdown.interactable = false;
        firstInputField.interactable = false;
        secondInputField.interactable = false;
        firstToggleDoubleUse.interactable = false;
        secondToggleDoubleUse.interactable = false;
        firstDoubleUseDropdown.interactable = false;
        secondDoubleUseDropdown.interactable = false;
    }

    public void SetHzRateFD(string hzString)
    {
        try
        {
            if (firstDeviceType == 1)
                readerWriter.measureEEGinHz = int.Parse(hzString);
            else
                readerWriter.measureONEEinHz = int.Parse(hzString);
        } catch(Exception e)
        {
            firstInputField.text = "retry";
        }
    }
    
    public void SetHzRateSD(string hzString)
    {
        try
        {
            if(secondDeviceType == 1)
                readerWriter.measureEEGinHz = int.Parse(hzString);
            else
                readerWriter.measureONEEinHz = int.Parse(hzString);
        } catch(Exception e)
        {
            secondInputField.text = "retry";
        }
    }

    public void SetHeaderNoConnection()
    {
        header.text = "No device found.";
    }
    
    
    public void OnPressWriteButton()
    {
        if (!readerWriter.write && !readerWriter.writeWasTrue)
        {
            readerWriter.write = true;
            readerWriter.writeWasTrue = true;
        } else if (readerWriter.write)
            readerWriter.write = false;
        else if (!readerWriter.write && readerWriter.writeWasTrue)
        {
            readerWriter.writtenEEG = true;
            readerWriter.writtenONEE = true;
            readerWriter.writtenTWOE = true;
        }
    }

    public void OnPressRecordButton()
    {
        if (!readerWriter.write && !readerWriter.writeWasTrue)
        {
            readerWriter.write = true;
            readerWriter.writeWasTrue = true;
            recordButtonText.text = "Stop Recording";
        } else if (readerWriter.write)
        {
            readerWriter.write = false;
            recordButton.interactable = false;
            writeButton.interactable = true;
        }
        else if (!readerWriter.write && readerWriter.writeWasTrue)
        {
            readerWriter.writtenEEG = true;
            readerWriter.writtenONEE = true;
            readerWriter.writtenTWOE = true;
        }
    }
}
