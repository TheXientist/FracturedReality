using Gtec.Gds.Client.API.Wrapper;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

public class SensorReaderFractured : MonoBehaviour
{
    public IPEndPoint remoteEndpoint;
    public IPEndPoint localEndpoint;
    public List<GdsClientApiLibraryWrapper.DeviceConnectionInfo> daqUnits;
    public Dictionary<string, GdsClientApiLibraryWrapper.GdsDeviceType> serialNumberAndType;
    public List<string> serialNumber;
    private UInt64 connectionHandle1 = UInt64.MinValue;
    private UInt64 connectionHandle2 = UInt64.MinValue;
    bool isCreator;
    public bool connectionEst = false;

    //Set necessary variables twice to read two devices:
    uint firstBufferSizeInSeconds = 5;
    uint firstSampleRate = 0;
    public ulong[] channelsForFirstDevice;
    ulong firstScanSizeInSamples;
    ulong firstBufferSizeInSamples;
    public bool[] firstEnabledChannels = new bool[] { false, false, false, false, false, false, false, false };

    uint secondBufferSizeInSeconds = 5;
    uint secondSampleRate = 0;
    public ulong[] channelsForSecondDevice;
    ulong secondScanSizeInSamples;
    ulong secondBufferSizeInSamples;
    public bool[] secondEnabledChannels = new bool[] { false, false, false, false, false, false, false, false };

    bool dataReadyFlag = false;
    GdsClientApiLibraryWrapper.GdsResultCallback DataAcquisitionErrorCallback;
    GdsClientApiLibraryWrapper.GdsCallback DataReadyCallback;
    GCHandle receiveBufferHandle1;
    GCHandle receiveBufferHandle2;

    //Container for sensor data:
    private float[] receiveBuffer1;
    private float[] receiveBuffer2;
    public static float[] eegDataArray = new float[8];
    private int eegDataArrayCounter;
    //public static Stack<float> stack2;

    //in case of single device use: measure one electrode (E8)
    public static float oneECurrentValue;
    //in case of double device use: measure two electrodes (E7 & E8)
    private bool doubleUseBool = false;
    public static float[] twoECurrentValue = new float[2];

    //deprecated container:
    //public static Queue<float> circBuffer;
    //public static Stack<float> stack1;

    //trasmit information between UI and SensorReader.
    //[SerializeField] private UIHandel uiHandel;
    [SerializeField] private WriteTime writeTime;

    // The amount of data to wait for before the "dataReadyFlag" triggered in milliseconds.
    const int DataReadyThresholdInMs = 30;

    //Variables for UI-less System
    [SerializeField] int hzRate = 5;


    // Start is called before the first frame update
    void Start()
    {
        startConnetion();
        Debug.Log("Sensor init started. Wait for connection...");
    }

    // Update is called once per frame
    void Update()
    {
        if (connectionEst)
        {
            readUpdate();
        }
    /*
        if (Input.GetKeyDown(KeyCode.J))
        {
            writeTime.measureONEEinHz = hzRate;
            ContinueDeviceSetup();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            OnPressWriteButton();
        }
    */
        if (Input.GetKeyDown(KeyCode.K))
        {
            OnPressRecordButton();
        }


    }

    private void OnDestroy()
    {

        if (connectionHandle1 != UInt64.MinValue)
        {
            //stop acquisition and streaming
            GdsClientApiLibraryWrapper.StopStreaming(connectionHandle1);
            GdsClientApiLibraryWrapper.StopAcquisition(connectionHandle1);
            //release allocated unmanaged resources
            receiveBufferHandle1.Free();
            //close the connection to the server
            GdsClientApiLibraryWrapper.Disconnect(ref connectionHandle1);
        }
        if (connectionHandle2 != UInt64.MinValue)
        {
            //stop acquisition and streaming
            GdsClientApiLibraryWrapper.StopStreaming(connectionHandle2);
            GdsClientApiLibraryWrapper.StopAcquisition(connectionHandle2);
            //release allocated unmanaged resources
            receiveBufferHandle2.Free();
            //close the connection to the server
            GdsClientApiLibraryWrapper.Disconnect(ref connectionHandle2);
        }

        //uninitialize the library
        GdsClientApiLibraryWrapper.Uninitialize();
    }

    public void startConnetion()
    {
        //StartCoroutine(InitConnection());
        InitConnection();
    }

    private async void InitConnection()
    {
        //yield return new WaitForSeconds(1f);
        
        DataReadyCallback = OnDataReadyCallback;
        DataAcquisitionErrorCallback = OnDataAcquisitionError;
        remoteEndpoint =
            new IPEndPoint(IPAddress.Loopback, 50223); //the address of the endpoint that runs the GDS server
        localEndpoint =
            new IPEndPoint(IPAddress.Loopback, 50224); //the address of the endpoint that runs the client/demo

        //initialize the library
        GdsClientApiLibraryWrapper.Initialize();
        await Task.Yield();

        try
        {
            //get connected devices:
            daqUnits = GdsClientApiLibraryWrapper.GetConnectedDevices(remoteEndpoint, localEndpoint);
            serialNumberAndType = PrintConnectedDevices(daqUnits, remoteEndpoint);
            serialNumber = dictConvert(serialNumberAndType);

            //communicate to UI the names of the devices:
            //uiHandel.SetTexts(serialNumber);
            //Debug.Log("SerialNumber" + serialNumber);
            

        }
        catch (GdsException ex)
        {
            //an error occurred while using the GDS Client API
            Debug.Log("ERROR #" + (int)ex.ErrorCode + " ('" + ex.ErrorCode + "'): " + ex.Message + "");
        }
        catch (DllNotFoundException)
        {
            //GDS device library is not found
            Debug.Log("The GDS device library is not found on the current path.");
        }
        catch (InvalidOperationException ex)
        {
            //no devices were found on the server
            Debug.Log(ex.Message);
            //uiHandel.SetHeaderNoConnection();
        }
        catch (ArgumentException ex)
        {
            Debug.Log("INVALID ARGUMENT ERROR: " + ex.Message);
        }
        catch (Exception ex)
        {
            Debug.Log("GENERAL ERROR:");
            Debug.Log(ex.StackTrace);
        }

        try
        {
            ContinueDeviceSetup();
            OnPressRecordButton();
        }
        catch 
        {
            Debug.Log("No sensors found");
        }

    }

    public void ContinueDeviceSetup()
    {
        //Deivce Types:
        // 0 - None
        // 1 - EEG
        // 2 - GSR
        // 3 - Pulse
        // 4 - Breathing

        int firstDeviceType = 2;
        int doubleDeviceType = 3;
        bool doubleDeviceUsage = true;
        writeTime.oneDevice = 2;

        writeTime.measureONEEinHz = hzRate;

        //not used here, do not change
        bool secondToggleDoubleUse = false;
        int secondDeviceType = 0;



        if (firstDeviceType != 0 || secondDeviceType != 0)
        {
            //Setup first device:
            //Special case EEG
            //if the first device is of Type EEG
            if (firstDeviceType == 1)
            {
                //measure 8 Electrodes
                firstEnabledChannels = new bool[] { true, true, true, true, true, true, true, true };

                connectionHandle1 = GdsClientApiLibraryWrapper.Connect(remoteEndpoint, localEndpoint,
                    new string[] { serialNumber[0] }, true, out isCreator);

                //connect callback to get notified about errors in data acquisition
                GdsClientApiLibraryWrapper.SetDataAcquisitionErrorCallback(connectionHandle1,
                    DataAcquisitionErrorCallback, IntPtr.Zero);

                //configure devices
                GUsbAmpGdsClientApiLibraryWrapper.GdsConfiguration[] deviceConfigurations =
                    gTekConfigs.CreateConfiguration(serialNumber[0], serialNumberAndType[serialNumber[0]],
                        out firstSampleRate, firstEnabledChannels);
                GdsClientApiLibraryWrapper.SetConfiguration(connectionHandle1, deviceConfigurations);

                //connect callback to trigger data processing each time when the specified amount of data is available
                GdsClientApiLibraryWrapper.SetDataReadyCallback(connectionHandle1, DataReadyCallback,
                    (ulong)(DataReadyThresholdInMs / (double)1000 * firstSampleRate), IntPtr.Zero);

                //determine size of a single scan
                GdsClientApiLibraryWrapper.GetDataInfo(connectionHandle1, 1, out channelsForFirstDevice,
                    out firstScanSizeInSamples);

                //create the (managed) buffer where a single block of received data should be written to and limit such a block to a specific amount of seconds and scans, respectively
                firstBufferSizeInSamples = firstBufferSizeInSeconds * firstSampleRate * firstScanSizeInSamples;
                receiveBuffer1 =
                    new float[firstBufferSizeInSamples]; //(a float array of size 'bufferSizeInSamples' could be used here as well, but the BinaryWriter used later only supports writing arrays of type byte)
                receiveBufferHandle1 = GCHandle.Alloc(receiveBuffer1, GCHandleType.Pinned);

                //start acquisition on the device on the server
                GdsClientApiLibraryWrapper.StartAcquisition(connectionHandle1);

                //start streaming in order to be able to receive data from the GDS server
                GdsClientApiLibraryWrapper.StartStreaming(connectionHandle1);
            }

            //if the first device is of Type GSR, Pulse or Breathing
            if (firstDeviceType >= 2)
            {
                if (doubleDeviceUsage == false)
                {
                    //only measure 8th Electrode
                    secondEnabledChannels = new bool[] { false, false, false, false, false, false, false, true };
                    writeTime.SetTypesAndUsage(doubleUseBool, firstDeviceType, 0);
                }
                else
                {
                    doubleUseBool = true;
                    secondEnabledChannels = new bool[] { false, false, false, false, false, false, true, true };
                    writeTime.SetTypesAndUsage(doubleUseBool, firstDeviceType, doubleDeviceType);
                }

                connectionHandle2 = GdsClientApiLibraryWrapper.Connect(remoteEndpoint, localEndpoint,
                    new string[] { serialNumber[0] }, true, out isCreator);

                //connect callback to get notified about errors in data acquisition
                GdsClientApiLibraryWrapper.SetDataAcquisitionErrorCallback(connectionHandle2,
                    DataAcquisitionErrorCallback, IntPtr.Zero);

                //configure devices
                GUsbAmpGdsClientApiLibraryWrapper.GdsConfiguration[] deviceConfigurations =
                    gTekConfigs.CreateConfiguration(serialNumber[0], serialNumberAndType[serialNumber[0]],
                        out secondSampleRate, secondEnabledChannels);
                GdsClientApiLibraryWrapper.SetConfiguration(connectionHandle2, deviceConfigurations);

                //connect callback to trigger data processing each time when the specified amount of data is available
                GdsClientApiLibraryWrapper.SetDataReadyCallback(connectionHandle2, DataReadyCallback,
                    (ulong)(DataReadyThresholdInMs / (double)1000 * secondSampleRate), IntPtr.Zero);

                //determine size of a single scan
                GdsClientApiLibraryWrapper.GetDataInfo(connectionHandle2, 1, out channelsForSecondDevice,
                    out secondScanSizeInSamples);

                //create the (managed) buffer where a single block of received data should be written to and limit such a block to a specific amount of seconds and scans, respectively
                secondBufferSizeInSamples = secondBufferSizeInSeconds * secondSampleRate * secondScanSizeInSamples;
                receiveBuffer2 =
                    new float[secondBufferSizeInSamples]; //(a float array of size 'bufferSizeInSamples' could be used here as well, but the BinaryWriter used later only supports writing arrays of type byte)
                receiveBufferHandle2 = GCHandle.Alloc(receiveBuffer2, GCHandleType.Pinned);

                //start acquisition on the device on the server
                GdsClientApiLibraryWrapper.StartAcquisition(connectionHandle2);

                //start streaming in order to be able to receive data from the GDS server
                GdsClientApiLibraryWrapper.StartStreaming(connectionHandle2);
            }


            //Setup second device:
            //if the second device is of Type EEG, first device cannot be overwritten
            if (secondDeviceType == 1 && firstDeviceType != 1)
            {
                //measure 8 Electrodes
                firstEnabledChannels = new bool[] { true, true, true, true, true, true, true, true };

                connectionHandle1 = GdsClientApiLibraryWrapper.Connect(remoteEndpoint, localEndpoint,
                    new string[] { serialNumber[1] }, true, out isCreator);

                //connect callback to get notified about errors in data acquisition
                GdsClientApiLibraryWrapper.SetDataAcquisitionErrorCallback(connectionHandle1,
                    DataAcquisitionErrorCallback, IntPtr.Zero);

                //configure devices
                GUsbAmpGdsClientApiLibraryWrapper.GdsConfiguration[] deviceConfigurations =
                    gTekConfigs.CreateConfiguration(serialNumber[1], serialNumberAndType[serialNumber[1]],
                        out firstSampleRate, firstEnabledChannels);
                GdsClientApiLibraryWrapper.SetConfiguration(connectionHandle1, deviceConfigurations);

                //connect callback to trigger data processing each time when the specified amount of data is available
                GdsClientApiLibraryWrapper.SetDataReadyCallback(connectionHandle1, DataReadyCallback,
                    (ulong)(DataReadyThresholdInMs / (double)1000 * firstSampleRate), IntPtr.Zero);

                //determine size of a single scan
                GdsClientApiLibraryWrapper.GetDataInfo(connectionHandle1, 1, out channelsForFirstDevice,
                    out firstScanSizeInSamples);

                //create the (managed) buffer where a single block of received data should be written to and limit such a block to a specific amount of seconds and scans, respectively
                firstBufferSizeInSamples = firstBufferSizeInSeconds * firstSampleRate * firstScanSizeInSamples;
                receiveBuffer1 =
                    new float[firstBufferSizeInSamples]; //(a float array of size 'bufferSizeInSamples' could be used here as well, but the BinaryWriter used later only supports writing arrays of type byte)
                receiveBufferHandle1 = GCHandle.Alloc(receiveBuffer1, GCHandleType.Pinned);

                //start acquisition on the device on the server
                GdsClientApiLibraryWrapper.StartAcquisition(connectionHandle1);

                //start streaming in order to be able to receive data from the GDS server
                GdsClientApiLibraryWrapper.StartStreaming(connectionHandle1);
            }

            //if the second device is of Type GSR, Pulse or Breathing - first device cannot be overwritten
            //if (uiHandel.secondDeviceType >= 2 && uiHandel.firstDeviceType != 2)
            if (secondDeviceType >= 2)
            {
                if (secondToggleDoubleUse == false)
                {
                    //only measure 8th Electrode
                    secondEnabledChannels = new bool[] { false, false, false, false, false, false, false, true };
                    writeTime.SetTypesAndUsage(doubleUseBool, secondDeviceType, 0);
                }
                else
                {
                    //measure 7th and 8th Electrode
                    doubleUseBool = true;
                    secondEnabledChannels = new bool[] { false, false, false, false, false, false, true, true };
                    writeTime.SetTypesAndUsage(doubleUseBool, secondDeviceType, doubleDeviceType);
                }

                connectionHandle2 = GdsClientApiLibraryWrapper.Connect(remoteEndpoint, localEndpoint,
                    new string[] { serialNumber[1] }, true, out isCreator);

                //connect callback to get notified about errors in data acquisition
                GdsClientApiLibraryWrapper.SetDataAcquisitionErrorCallback(connectionHandle2,
                    DataAcquisitionErrorCallback, IntPtr.Zero);

                //configure devices
                GUsbAmpGdsClientApiLibraryWrapper.GdsConfiguration[] deviceConfigurations =
                    gTekConfigs.CreateConfiguration(serialNumber[1], serialNumberAndType[serialNumber[1]],
                        out secondSampleRate, secondEnabledChannels);
                GdsClientApiLibraryWrapper.SetConfiguration(connectionHandle2, deviceConfigurations);

                //connect callback to trigger data processing each time when the specified amount of data is available
                GdsClientApiLibraryWrapper.SetDataReadyCallback(connectionHandle2, DataReadyCallback,
                    (ulong)(DataReadyThresholdInMs / (double)1000 * secondSampleRate), IntPtr.Zero);

                //determine size of a single scan
                GdsClientApiLibraryWrapper.GetDataInfo(connectionHandle2, 1, out channelsForSecondDevice,
                    out secondScanSizeInSamples);

                //create the (managed) buffer where a single block of received data should be written to and limit such a block to a specific amount of seconds and scans, respectively
                secondBufferSizeInSamples = secondBufferSizeInSeconds * secondSampleRate * secondScanSizeInSamples;
                receiveBuffer2 =
                    new float[secondBufferSizeInSamples]; //(a float array of size 'bufferSizeInSamples' could be used here as well, but the BinaryWriter used later only supports writing arrays of type byte)
                receiveBufferHandle2 = GCHandle.Alloc(receiveBuffer2, GCHandleType.Pinned);

                //start acquisition on the device on the server
                GdsClientApiLibraryWrapper.StartAcquisition(connectionHandle2);

                //start streaming in order to be able to receive data from the GDS server
                GdsClientApiLibraryWrapper.StartStreaming(connectionHandle2);
            }
            connectionEst = true;
        }
        else
        {
            Debug.Log("No GDS Sensor Devices Found!");
        }

        Debug.Log("Continue Successfull");
    }

    void readUpdate()
    {
        if (dataReadyFlag)
        {
            dataReadyFlag = false;
            ulong firstScanCount = 0;
            ulong secondScanCount = 0;

            if (connectionHandle1 != UInt64.MinValue)
            {
                ulong receivedScans1 = GdsClientApiLibraryWrapper.GetData(connectionHandle1, firstScanCount,
                    receiveBufferHandle1.AddrOfPinnedObject(), firstBufferSizeInSamples);

                /*for(int i = 0; i < (int)receivedScans1 * (int)firstScanSizeInSamples; i++)
                {
                    eegDataArray[eegDataArrayCounter] = receiveBuffer1[i];
                    
                    if (eegDataArrayCounter < 8)
                        eegDataArrayCounter++;
                    else
                        eegDataArrayCounter = 0;
                    
                }*/
                try
                {
                    for (int i = 0; i < 8; i++)
                    {
                        eegDataArray[i] = receiveBuffer1[i];
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("not writing device 1");
                }
            }

            if (connectionHandle2 != UInt64.MinValue)
            {
                ulong receivedScans2 = GdsClientApiLibraryWrapper.GetData(connectionHandle2, secondScanCount,
                    receiveBufferHandle2.AddrOfPinnedObject(), secondBufferSizeInSamples);

                /*
                for (int i = 0; i < (int) receivedScans2 * (int) secondScanSizeInSamples; i++)
                {
                    stack2.Push(receiveBuffer2[i]);
                }
                */
                //Debug.Log(receiveBuffer2[0]);
                if (!doubleUseBool)
                {
                    try
                    {
                        //stack2.Push(receiveBuffer2[0]);
                        oneECurrentValue = receiveBuffer2[0];
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("not writing device 2 (One Electrode)");
                    }
                }
                else
                {
                    try
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            twoECurrentValue[i] = receiveBuffer2[i];
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("not writing device 2 (Double Use)");
                    }
                }
            }

            //Debug.Log("|"+ string.Join(" ", receiveBuffer1)+"|");
            //Debug.Log("|"+ string.Join(" ", receiveBuffer2)+"|");
        }

    }

    /// <summary>
    /// Writes the list of devices that are currently connected to the server out to the console.
    /// </summary>
    /// <param name="daqUnits">The data structure which holds the device serial number as well as the device type.</param>
    /// <param name="remoteEndpoint">The endpoint of the GDS. This parameter is used only to print additional information on the console.</param>
    /// <remarks>Devices in use are grouped by the data acquisition unit they belong to.</remarks>
    /// <returns>A dictionary which maps the device serial number to its according device type.</returns>
    static Dictionary<string, GdsClientApiLibraryWrapper.GdsDeviceType> PrintConnectedDevices(List<GdsClientApiLibraryWrapper.DeviceConnectionInfo> daqUnits, IPEndPoint remoteEndpoint)
    {
        if (daqUnits.Count == 0)
            throw new InvalidOperationException(
                $"No devices are connected to GDS server at endpoint '{remoteEndpoint.ToString()}'");

        //print device list
        Debug.Log("The following devices are connected to GDS server at endpoint '" + remoteEndpoint.ToString() + "':");

        Dictionary<string, GdsClientApiLibraryWrapper.GdsDeviceType> serialNumberAndType = new Dictionary<string, GdsClientApiLibraryWrapper.GdsDeviceType>();
        foreach (GdsClientApiLibraryWrapper.DeviceConnectionInfo daqUnit in daqUnits)
        {
            Debug.Log(daqUnit.InUse
                ? "  Currently in use by a separate DAQ unit:"
                : "  Connected, but currently not in use:");

            foreach (GdsClientApiLibraryWrapper.DeviceInfo connectedDevice in daqUnit.ConnectedDevices)
            {
                serialNumberAndType.Add(connectedDevice.Name, connectedDevice.DeviceType);
                Debug.Log("\t" + connectedDevice.Name + "\t(" + connectedDevice.DeviceType.ToString() + ")");
            }
        }
        return serialNumberAndType;
    }

    static List<string> dictConvert(Dictionary<string, GdsClientApiLibraryWrapper.GdsDeviceType> inDict)
    {
        List<string> ret = new List<string>();

        if (inDict != null)
        {
            foreach (KeyValuePair<string, GdsClientApiLibraryWrapper.GdsDeviceType> item in inDict)
            {
                ret.Add(item.Key);
            }
        }
        return ret;
    }

    /// <summary>
    /// Handles the GDS's <b>DataAcquisitionError</b> callback.
    /// Just prints information about the error that occurred. An application might react on the occurred error properly.
    /// </summary>
    /// <param name="connectionHandle">The connection handle for which the callback was triggered.</param>
    /// <param name="error">Details on the error that occurred.</param>
    /// <param name="userData">A pointer to an unmanaged block of memory containing the custom user data specified when setting the callback.</param>
    void OnDataAcquisitionError(UInt64 connectionHandle, GdsClientApiLibraryWrapper.GdsResult error, IntPtr userData)
    {
        Debug.Log("DATA ACQUISITION ERROR #" + error.ErrorCode + ": " + error.ErrorMessage);
    }

    /// <summary>
    /// Handles the GDS's <b>DataReady</b> callback.
    /// Signals the application that the requested amount of data is available now.
    /// </summary>
    /// <param name="connectionHandle">The connection handle for which the callback was triggered.</param>
    /// <param name="userData">A pointer to an unmanaged block of memory containing the custom user data specified when setting the callback.</param>
    void OnDataReadyCallback(UInt64 connectionHandle, IntPtr userData)
    {
        dataReadyFlag = true;
    }

    public void OnPressWriteButton()
    {
        if (!writeTime.write && !writeTime.writeWasTrue)
        {
            writeTime.write = true;
            writeTime.writeWasTrue = true;
        }
        else if (writeTime.write)
            writeTime.write = false;
        else if (!writeTime.write && writeTime.writeWasTrue)
        {
            writeTime.writtenEEG = true;
            writeTime.writtenONEE = true;
            writeTime.writtenTWOE = true;
        }

        Debug.Log("Write Successfull");
    }

    public void OnPressRecordButton()
    {
        if (!writeTime.write && !writeTime.writeWasTrue)
        {
            writeTime.write = true;
            writeTime.writeWasTrue = true;
            //recordButtonText.text = "Stop Recording";
        }
        else if (writeTime.write)
        {
            writeTime.write = false;
            //recordButton.interactable = false;
            //writeButton.interactable = true;
            OnPressWriteButton();
        }
        else if (!writeTime.write && writeTime.writeWasTrue)
        {
            writeTime.writtenEEG = true;
            writeTime.writtenONEE = true;
            writeTime.writtenTWOE = true;
        }

        Debug.Log("Record Successfull, press K to stop recording");
    }


}
