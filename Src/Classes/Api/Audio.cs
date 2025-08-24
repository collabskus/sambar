/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using NAudio.Wave;
using NAudio.CoreAudioApi;
namespace sambar;

public partial class Api
{
    bool isAudioPlaying = false;
    WasapiLoopbackCapture systemAudioCapture = new();
    MMDevice? defaultSpeaker = null;
    private void AudioInit() 
    {
        MMDeviceEnumerator deviceEnumerator = new();
        defaultSpeaker = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        Logger.Log("defaultSpeaker: " + defaultSpeaker?.AudioMeterInformation?.PeakValues[0].ToString());

        System.Timers.Timer audioTimer = new(100);
        audioTimer.Elapsed += AudioTimer_Elapsed;
        audioTimer.Start();
    }

    private void AudioTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (defaultSpeaker?.AudioMeterInformation?.PeakValues[0] > 0)
        {
            if(!isAudioPlaying)
            {
                isAudioPlaying = true;
                systemAudioCapture.DataAvailable += SystemAudioCapture_DataAvailable;
                systemAudioCapture.StartRecording();
            }
            Logger.Log("Audio playing...");
        }
        else
        {
            if (isAudioPlaying)
            {
                isAudioPlaying = false;
                systemAudioCapture.StopRecording();
                systemAudioCapture.DataAvailable -= SystemAudioCapture_DataAvailable;
            }
            Logger.Log("Audio stopped...");
        }
    }

    private void SystemAudioCapture_DataAvailable(object? sender, WaveInEventArgs e)
    {
        //byte[] buffer = e.Buffer;
    }
}
