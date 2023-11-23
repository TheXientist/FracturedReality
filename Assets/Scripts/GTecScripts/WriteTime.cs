using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine;
using System.IO;
using UnityEngine.Serialization;

//using System.Text.Json;

public class WriteTime : MonoBehaviour
{
    private ONEEntry oneeEntry;
    public List<ONEEntry> oneeFile;
    private SECEEntry seceEntry;
    public List<SECEEntry> seceFile;
    private EEGEntry eegEntry;
    public List<EEGEntry> eegFile;
    public bool write = false;
    public bool writeWasTrue = false;
    public bool writtenEEG = false;
    public bool writtenONEE = false;
    public bool writtenTWOE = false;
    public float measureEEGinHz = 1.0f;
    public float measureONEEinHz = 1.0f;
    public bool inEEGCoroutine = false;
    public bool inONEECoroutine = false;
    public bool inTWOECoroutine = false;
    private Stack<float> copyStack;
    private float dataConverter;

    private float dataConverter2;

    //oneDevice takes device Type if there is only one device connected, else has value 0;
    public int oneDevice;

    //Sensor Types
    private bool isDoubleUse;
    private float twoeDeviceTypeOne = 0.0f;
    private float twoeDeviceTypeTwo = 0.0f;
    private string twoeDeviceStrngOne;
    private string twoeDeviceStringTwo;

    private void Start()
    {
        oneeEntry = new ONEEntry();
        oneeFile = new List<ONEEntry>();

        seceEntry = new SECEEntry();
        seceFile = new List<SECEEntry>();

        eegEntry = new EEGEntry();
        eegFile = new List<EEGEntry>();

        oneDevice = 3;
    }

    //Is called by SensorReader to state:
    //- if the device is in double Use - Two electrodes are measured for two sensor types
    //- what the senor types are
    public void SetTypesAndUsage(bool doubleUse, int firstUseType, int secondUseType)
    {
        isDoubleUse = doubleUse;

        if (firstUseType == 2)
        {
            twoeDeviceTypeOne = 0.00015f;
            twoeDeviceStrngOne = "GSR";
        }
        else if (firstUseType == 3)
        {
            twoeDeviceTypeOne = 1.0f; //TEMPORARY VALUE
            twoeDeviceStrngOne = "PULSE";
        }
        else if (firstUseType == 4)
        {
            twoeDeviceTypeOne = 1.0f; //TEMPORARY VALUE
            twoeDeviceStrngOne = "BREATH";
        }
        else
        {
            Debug.LogWarning("Invalid device Type: twoeDeviceTypeOne");
        }

        if (secondUseType == 0)
        {
            //nothing :)
        }
        else if (secondUseType == 2)
        {
            twoeDeviceTypeTwo = 0.00015f;
            twoeDeviceStringTwo = "GSR";
        }
        else if (secondUseType == 3)
        {
            twoeDeviceTypeTwo = 1.0f; //TEMPORARY VALUE
            twoeDeviceStringTwo = "PULSE";
        }
        else if (secondUseType == 4)
        {
            twoeDeviceTypeTwo = 1.0f; //TEMPORARY VALUE
            twoeDeviceStringTwo = "BREATH";
        }
        else
        {
            Debug.LogWarning("Invalid device Type: twoeDeviceTypeOne");
        }
    }

    // Update is called once per frame
    //case 0: not ONE device is measured but TWO
    //case 1: ONE device is measured, its EEG
    //case 2: ONE device is measured, its a ONE-Electrode Device
    private void Update()
    {

        if (write || writtenEEG || writtenONEE || writtenTWOE)
        {
            if (oneDevice == 0)
            {
                if (!inEEGCoroutine)
                    StartCoroutine(WriteEEGFileCoroutine());
                if (!isDoubleUse)
                {
                    if (!inONEECoroutine)
                    {
                        StartCoroutine(WriteONEEFileCoroutine());
                    }
                }
                else if(!inTWOECoroutine)
                {
                        StartCoroutine(WriteTWOEFileCoroutine());
                }

            }
            else if (oneDevice == 1)
            {
                writtenONEE = false;
                writtenTWOE = false;
                if (!inEEGCoroutine)
                    StartCoroutine(WriteEEGFileCoroutine());
            }
            else if (oneDevice == 2)
            {
                writtenEEG = false;
                if (!isDoubleUse)
                {
                    if (!inONEECoroutine)
                    {
                        StartCoroutine(WriteONEEFileCoroutine());
                    }
                }
                else if (!inTWOECoroutine)
                {
                    StartCoroutine(WriteTWOEFileCoroutine());
                }
                
            }
        }
    }

    IEnumerator WriteEEGFileCoroutine()
    {
        inEEGCoroutine = true;
        if (write)
        {
            DateTime dt = DateTime.Now;
            //string dateTime = dt.Hour.ToString() + ":" + dt.Minute.ToString() + ":" + dt.Second.ToString() + ":" + dt.Millisecond.ToString();
            string time = dt.ToString("hh:mm:ss:fff"); //dt.Hour.ToString() + ":" + dt.Minute.ToString() + ":" + dt.Second.ToString() + ":" + dt.Millisecond.ToString();
            eegEntry.time = time;

            //EEG Stuff
            float[] copyEEGArray = new float[8];
            try
            {
                copyEEGArray = SensorReader.eegDataArray;

            }
            catch (Exception ex)
            {
                copyEEGArray = new float[] {-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f};
            }

            eegEntry.Electrode1 = copyEEGArray[0].ToString();
            eegEntry.Electrode2 = copyEEGArray[1].ToString();
            eegEntry.Electrode3 = copyEEGArray[2].ToString();
            eegEntry.Electrode4 = copyEEGArray[3].ToString();
            eegEntry.Electrode5 = copyEEGArray[4].ToString();
            eegEntry.Electrode6 = copyEEGArray[5].ToString();
            eegEntry.Electrode7 = copyEEGArray[6].ToString();
            eegEntry.Electrode8 = copyEEGArray[7].ToString();

            eegFile.Add(eegEntry);
            eegEntry = new EEGEntry();
            //old
            //gsrDataConvert = SensorReader.stack.Pop() / 1.000283
            //jsonEntry.GSRdata = SensorReader.circBuffer.Dequeue().ToString();
            //jsonEntry.GSRdata = SensorReader.stack.Pop().ToString();
            //jsonEntry.GSRdata2 = SensorReader.stack2.Pop().ToString();
            //SensorReader.circBuffer.Clear();

        }
        else if (writtenEEG)
        {
            writtenEEG = false;
            string eegJson = JsonHelper.ToJson(eegFile, true);
            //File.WriteAllText(Application.dataPath, eegJson);
            string path = "Assets/TestData/EEG-" + DateTime.Now.ToString("%d%M-hhmmss") + ".json";
            File.WriteAllText(path, eegJson);
        }

        yield return new WaitForSeconds(1 / measureEEGinHz);
        if (write || writtenEEG)
        {
            StartCoroutine(WriteEEGFileCoroutine());
        }
        else
        {
            inEEGCoroutine = false;
        }
    }

    IEnumerator WriteONEEFileCoroutine()
    {
        inONEECoroutine = true;
        if (write)
        {
            DateTime dt = DateTime.Now;
            //string dateTime = dt.Hour.ToString() + ":" + dt.Minute.ToString() + ":" + dt.Second.ToString() + ":" + dt.Millisecond.ToString();
            string time = dt.ToString("hh:mm:ss:fff");//dt.Hour.ToString() + ":" + dt.Minute.ToString() + ":" + dt.Second.ToString() + ":" + dt.Millisecond.ToString();
            oneeEntry.time = time;

            //GSR stuff
            //MODIFY: TYP Abfrage
            try
            {
                //float gsrDataConvert = SensorReader.stack2.Pop() * 0.00015f;
                //dataConverter = SensorReader.oneECurrentValue * 0.00015f;
                //conversion is stored in the device type variable!
                dataConverter = SensorReaderFractured.oneECurrentValue * twoeDeviceTypeOne;
            }
            catch (Exception ex)
            {
                dataConverter = -1.0f;
            }

            oneeEntry.ONEEdata = dataConverter.ToString();
            oneeFile.Add(oneeEntry);
            oneeEntry = new ONEEntry();

        }
        else if (writtenONEE)
        {
            writtenONEE = false;
            string oneeJson = JsonHelper.ToJson(oneeFile, true);
            string path = "Assets/GTecData/" + twoeDeviceStrngOne + "-" + DateTime.Now.ToString("%d%M-hhmmss") +
                          ".json";

            try
            {
                File.WriteAllText(path, oneeJson);
            }
            catch (Exception e)
            {
                Debug.LogError("ONEE not written :(");
            }
        }
        
        yield return new WaitForSeconds(1 / measureONEEinHz);
        if (write || writtenONEE)
        {
            StartCoroutine(WriteONEEFileCoroutine());
        }
        else
        {
            inONEECoroutine = false;
        }
    }

    IEnumerator WriteTWOEFileCoroutine()
    {
        inTWOECoroutine = true;
        if (write)
        {
            DateTime dt = DateTime.Now;
            string time = dt.ToString("hh:mm:ss:fff");//dt.Hour.ToString() + ":" + dt.Minute.ToString() + ":" + dt.Second.ToString() + ":" + dt.Millisecond.ToString();
            oneeEntry.time = time;
            seceEntry.time = time;

            try
            {
                dataConverter = SensorReader.twoECurrentValue[0] * twoeDeviceTypeOne;
                dataConverter2 = SensorReader.twoECurrentValue[1] * twoeDeviceTypeTwo;
            }
            catch (Exception ex)
            {
                dataConverter = -1.0f;
            }

            oneeEntry.ONEEdata = dataConverter.ToString();
            seceEntry.SECEdata = dataConverter2.ToString();
            oneeFile.Add(oneeEntry);
            seceFile.Add(seceEntry);
            oneeEntry = new ONEEntry();
            seceEntry = new SECEEntry();

        }
        else if (writtenTWOE)
        {
            writtenTWOE = false;
            string oneeJson = JsonHelper.ToJson(oneeFile, true);
            string twoeeJson = JsonHelper.ToJson(seceFile, true);

            string path = "Assets/GTecData/" + twoeDeviceStrngOne + "-" + DateTime.Now.ToString("%d%M-hhmmss") +
                          ".json";
            string path2 = "Assets/GTecData/" + twoeDeviceStringTwo + "-" + DateTime.Now.ToString("%d%M-hhmmss") +
                           ".json";
            try
            {
                File.WriteAllText(path, oneeJson);
                File.WriteAllText(path2, twoeeJson);
            }
            catch (Exception e)
            {
                Debug.LogError("At least one of two electrodes was not written :(");
            }
        }

        yield return new WaitForSeconds(1 / measureONEEinHz);
        if (write || writtenTWOE)
        {
            StartCoroutine(WriteTWOEFileCoroutine());
        }
        else
        {
            inTWOECoroutine = false;
        }
    }
}



