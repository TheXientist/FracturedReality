using Gtec.Gds.Client.API.Wrapper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gTekConfigs 
{
    /// <summary>
    /// Initializes a default configuration for a g.USBamp device to use with GDS.
    /// </summary>
    /// <param name="serialNumber">The serial number of the g.USBamp device to use.</param>
    /// <param name="sampleRate">The sample rate of the device to use.</param>
    /// <returns>An initialized g.USBamp device configuration to use with GDS.</returns>
    static GUsbAmpGdsClientApiLibraryWrapper.GdsConfiguration CreateGUsbAmpConfiguration(string serialNumber, out uint sampleRate, bool[] enabledChannels)
    {
        sampleRate = 256;
        const int NumberOfAcquiredChannels = 16;            //valid values range from 1...16

        //initialize device configuration structure
        GUsbAmpGdsClientApiLibraryWrapper.GdsGUsbAmpConfiguration deviceConfiguration = new GUsbAmpGdsClientApiLibraryWrapper.GdsGUsbAmpConfiguration();

        deviceConfiguration.SampleRate = sampleRate;
        deviceConfiguration.NumberOfScans = (UIntPtr)0;    //use recommended number of scans by setting this value to zero
        deviceConfiguration.TriggerEnabled = true;
        deviceConfiguration.CounterEnabled = false;
        deviceConfiguration.ShortCutEnabled = false;
        deviceConfiguration.CommonGround = new bool[4] { false, false, false, false };
        deviceConfiguration.CommonReference = new bool[4] { false, false, false, false };

        deviceConfiguration.InternalSignalGenerator = new GUsbAmpGdsClientApiLibraryWrapper.GdsGUsbAmpSignalGeneratorConfiguration();
        deviceConfiguration.InternalSignalGenerator.Enabled = false;
        deviceConfiguration.InternalSignalGenerator.WaveShape = GUsbAmpGdsClientApiLibraryWrapper.GdsGUsbAmpWaveShape.Sine;
        deviceConfiguration.InternalSignalGenerator.Frequency = 10;
        deviceConfiguration.InternalSignalGenerator.Amplitude = 1;
        deviceConfiguration.InternalSignalGenerator.Offset = 0;

        deviceConfiguration.Channels = new GUsbAmpGdsClientApiLibraryWrapper.GdsGUsbAmpChannelConfiguration[GUsbAmpGdsClientApiLibraryWrapper.MaxNumberOfChannels];

        for (int i = 0; i < deviceConfiguration.Channels.Length; i++)
        {
            deviceConfiguration.Channels[i].Acquire = (i < enabledChannels.Length) ? enabledChannels[i] : false;
            deviceConfiguration.Channels[i].BandpassFilterIndex = -1;                   //the one-based index of the g.USBamp binary filter table (index 42 == bandpass 0.1-30 Hz, zero indicates 'no filter'
            deviceConfiguration.Channels[i].NotchFilterIndex = -1;                      //the one-based index of the g.USBamp binary filter table (index 2 == notch 50Hz), zero indicates 'no filter'
            deviceConfiguration.Channels[i].BipolarChannel = 0;
        }

        //build and return complete GDS configuration structure
        GdsClientApiLibraryWrapper.GdsConfiguration gdsConfig = new GdsClientApiLibraryWrapper.GdsConfiguration();

        gdsConfig.DeviceConfiguration = deviceConfiguration;
        gdsConfig.DeviceInfo = new GdsClientApiLibraryWrapper.DeviceInfo();
        gdsConfig.DeviceInfo.DeviceType = GdsClientApiLibraryWrapper.GdsDeviceType.GUsbAmp;
        gdsConfig.DeviceInfo.Name = serialNumber;

        return gdsConfig;
    }

    /// <summary>
    /// Initializes a default configuration for a g.USBamp device to use with GDS.
    /// </summary>
    /// <param name="serialNumber">The serial number of the g.HIamp device to use.</param>
    /// <param name="sampleRate">The sample rate of the device to use.</param>
    /// <returns>An initialized g.HIamp device configuration to use with GDS.</returns>
    static GHiAmpGdsClientApiLibraryWrapper.GdsConfiguration CreateGHiAmpConfiguration(string serialNumber, out uint sampleRate, bool[] enabledChannels)
    {
        sampleRate = 256;
        const int numberOfAcquiredChannels = 40; //valid values range from 1...256

        GHiAmpGdsClientApiLibraryWrapper.GdsGHiAmpConfiguration deviceConfiguration = new GHiAmpGdsClientApiLibraryWrapper.GdsGHiAmpConfiguration();
        deviceConfiguration.SamplingRate = sampleRate;
        deviceConfiguration.NumberOfScans = 0;
        deviceConfiguration.HoldEnabled = false;
        deviceConfiguration.TriggerLinesEnabled = false;
        deviceConfiguration.CounterEnabled = true;
        deviceConfiguration.InternalSignalGenerator.Enabled = true;
        deviceConfiguration.InternalSignalGenerator.Frequency = 10;

        deviceConfiguration.Channels = new GHiAmpGdsClientApiLibraryWrapper.GdsGHiAmpChannelConfiguration[GHiAmpGdsClientApiLibraryWrapper.MaxNumberOfChannels];

        for (int i = 0; i < GHiAmpGdsClientApiLibraryWrapper.MaxNumberOfChannels; i++)
        {
            deviceConfiguration.Channels[i].Acquire = (i < enabledChannels.Length) ? enabledChannels[i] : false;
            deviceConfiguration.Channels[i].BandpassFilterIndex = -1;
            deviceConfiguration.Channels[i].NotchFilterIndex = -1;
            deviceConfiguration.Channels[i].ReferenceChannel = 0;
        }

        GdsClientApiLibraryWrapper.GdsConfiguration gdsConfig = new GdsClientApiLibraryWrapper.GdsConfiguration();

        gdsConfig.DeviceConfiguration = deviceConfiguration;
        gdsConfig.DeviceInfo = new GdsClientApiLibraryWrapper.DeviceInfo();
        gdsConfig.DeviceInfo.DeviceType = GdsClientApiLibraryWrapper.GdsDeviceType.GHiAmp;
        gdsConfig.DeviceInfo.Name = serialNumber;

        return gdsConfig;
    }

    /// <summary>
    /// Initializes a default configuration for a g.Nautilus device to use with GDS.
    /// </summary>
    /// <param name="serialNumber">The serial number of the g.Nautilus device to use.</param>
    /// <param name="sampleRate">The sample rate of the device to use.</param>
    /// <returns>An initialized g.Nautilus device configuration to use with GDS.</returns>
    static GNautilusGdsClientApiLibraryWrapper.GdsConfiguration CreateGNautilusConfiguration(string serialNumber, out uint sampleRate, bool[] enabledChannels)
    {
        sampleRate = 500;
        const int numberOfAcquiredChannels = 8; //valid values range from 1...8 (for a single module)
        const double sensitivity = 187500.0;

        GNautilusGdsClientApiLibraryWrapper.GdsGNautilusConfiguration deviceConfiguration = new GNautilusGdsClientApiLibraryWrapper.GdsGNautilusConfiguration();
        deviceConfiguration.IsSlave = false;
        deviceConfiguration.SamplingRate = sampleRate;
        deviceConfiguration.NumberOfScans = 0;
        deviceConfiguration.NetworkChannel = 0;
        deviceConfiguration.DigitalIOs = false;
        deviceConfiguration.InputSignal = GNautilusGdsClientApiLibraryWrapper.InputSignal.Electrode;
        deviceConfiguration.AccelerationData = false;
        deviceConfiguration.BatteryLevel = false;
        deviceConfiguration.CAR = false;
        deviceConfiguration.Counter = false;
        deviceConfiguration.LinkQualityInformation = false;
        deviceConfiguration.ValidationIndicator = false;
        deviceConfiguration.NoiseReduction = false;

        deviceConfiguration.Channels = new GNautilusGdsClientApiLibraryWrapper.GdsGNautilusChannelConfiguration[GNautilusGdsClientApiLibraryWrapper.MaxNumberOfChannels];

        for (int i = 0; i < GNautilusGdsClientApiLibraryWrapper.MaxNumberOfChannels; i++)
        {
            deviceConfiguration.Channels[i].Enabled = (i < enabledChannels.Length) && enabledChannels[i];
            deviceConfiguration.Channels[i].Sensitivity = sensitivity;
            deviceConfiguration.Channels[i].UsedForNoiseReduction = false;
            deviceConfiguration.Channels[i].BandpassFilterIndex = -1;
            deviceConfiguration.Channels[i].NotchFilterIndex = -1;
            deviceConfiguration.Channels[i].BipolarChannel = -1;
        }

        GdsClientApiLibraryWrapper.GdsConfiguration gdsConfig = new GdsClientApiLibraryWrapper.GdsConfiguration();

        gdsConfig.DeviceConfiguration = deviceConfiguration;
        gdsConfig.DeviceInfo = new GdsClientApiLibraryWrapper.DeviceInfo();
        gdsConfig.DeviceInfo.DeviceType = GdsClientApiLibraryWrapper.GdsDeviceType.GNautilus;
        gdsConfig.DeviceInfo.Name = serialNumber;

        return gdsConfig;
    }

    /// <summary>
    /// Initalizes a default device configuration according to the given device type.
    /// </summary>
    /// <param name="serialNumber">The serial number (i.e. the device name) of the device.</param>
    /// <param name="deviceType">The device type according to the serialnumber.</param>
    /// <param name="sampleRate">The sample rate of the device to use.</param>
    /// <returns>An initialized default configuration for the device.</returns>
    public static GdsClientApiLibraryWrapper.GdsConfiguration[] CreateConfiguration(string serialNumber, GdsClientApiLibraryWrapper.GdsDeviceType deviceType, out uint sampleRate, bool[] enabledChannels)
    {
        switch (deviceType)
        {
            case GdsClientApiLibraryWrapper.GdsDeviceType.GUsbAmp:
                return new GUsbAmpGdsClientApiLibraryWrapper.GdsConfiguration[] { CreateGUsbAmpConfiguration(serialNumber, out sampleRate, enabledChannels) };
            case GdsClientApiLibraryWrapper.GdsDeviceType.GHiAmp:
                return new GUsbAmpGdsClientApiLibraryWrapper.GdsConfiguration[] { CreateGHiAmpConfiguration(serialNumber, out sampleRate, enabledChannels) };
            case GdsClientApiLibraryWrapper.GdsDeviceType.GNautilus:
                return new GNautilusGdsClientApiLibraryWrapper.GdsConfiguration[] { CreateGNautilusConfiguration(serialNumber, out sampleRate, enabledChannels) };
            default:
                throw new ArgumentException("The device " + serialNumber + " of type " + deviceType.ToString() + " is not supported yet.");
        }
    }
}
