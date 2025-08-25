/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using NAudio.Wave;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using ScottPlot;
using ScottPlot.WPF;
using System.Windows;

namespace sambar;

public partial class Api
{
    WasapiLoopbackCapture systemAudioCapture = new();
    AudioMeterInformation? audioMeterInformation = null;

    const int SAMPLE_RATE = 44100;
    const int BITS = 16;
    const int CHANNELS = 2;
    const int SAMPLE_WIDTH = (BITS/sizeof(byte)) * CHANNELS;
    const int TIME_SLICE = 100;
    const int SAMPLES_IN_TIME_SLICE = SAMPLE_RATE * TIME_SLICE / 1000;
    const int BYTES_IN_TIME_SLICE = SAMPLES_IN_TIME_SLICE * SAMPLE_WIDTH * (BITS / sizeof(byte));

    WaveFormat waveFormat;
    
    List<double> amplitudes = new();
    WpfPlot plot = new();
    private void AudioInit() 
    {
        waveFormat = new(SAMPLE_RATE, BITS, CHANNELS); 
        systemAudioCapture.WaveFormat = waveFormat;

        MMDeviceEnumerator deviceEnumerator = new();
        MMDevice? defaultSpeaker = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        audioMeterInformation = defaultSpeaker?.AudioMeterInformation;

        System.Timers.Timer audioTimer = new(TIME_SLICE);
        audioTimer.Elapsed += AudioTimer_Elapsed;
        audioTimer.Start();

        //amplitudes = new double[SAMPLES_IN_TIME_SLICE];
        plot.Plot.Add.Signal(amplitudes, SAMPLE_RATE/1000);
        CreateLogWindow(plot);
    }

    private void AudioTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if(audioMeterInformation?.MasterPeakValue > 0)
        {
            if(systemAudioCapture.CaptureState == CaptureState.Stopped)
            {
                systemAudioCapture.DataAvailable += SystemAudioCapture_DataAvailable;
                systemAudioCapture.StartRecording();
            }
            Logger.Log($"Audio Playing ... ");
        }
        else
        {
            if(systemAudioCapture.CaptureState == CaptureState.Capturing)
            {
                systemAudioCapture.StopRecording();
                systemAudioCapture.DataAvailable -= SystemAudioCapture_DataAvailable;
            }
            Logger.Log($"Audio Stopped... ");
        }
    }

    private void SystemAudioCapture_DataAvailable(object? sender, WaveInEventArgs e)
    {
        byte[] bytes = e.Buffer.Take(e.BytesRecorded).ToArray();
        float[] samples = GetSamples(bytes);
        for (int i = 0; i < samples.Length; i++)
        {
            if (i >= amplitudes.Count) amplitudes.Add(samples[i]);
            else { amplitudes[i] = samples[i]; }
        }
        amplitudes.RemoveRange(samples.Length - 1, amplitudes.Count - samples.Length);
        plot.Plot.Axes.SetLimitsY(-1, 1);
        plot.Plot.Axes.AutoScaleX();
        plot.Refresh();
    }

    //private void UpdateWhenTimeSliceFilled(byte[] bytes)
    //{
    //    if()
    //    {
    //        UpdatePlot();
    //    }
    //    else
    //    {

    //    }
    //}
    
    // Samples are the amplitudes
    private float[] GetSamples(byte[] bytes)
    {
        // total bytes recorded = samples * bytes per sample * channels
        // 16 bit = 2 bytes => bytes per sample
        int samplesCount = bytes.Length / SAMPLE_WIDTH;

        // each sample has an amplitude
        float[] samples = new float[samplesCount];
        
        // for simplicity choose the first channel
        for(int i = 0; i < samplesCount; i++)
        {
            // [ ..., 0x00, 0x10, ...]
            //        ^(i)  ^(i+1) 
            //            <-| shift to the left halfway (16/2 = 8 bits)
            short sample = (short)(bytes[i*SAMPLE_WIDTH] | bytes[i*SAMPLE_WIDTH+ 1] << 8);
            samples[i] = sample / 32768f;
        }

        return samples;
    }
}
