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
    

    int sampleRate = 44100;
    int bits = 16;
    int channels = 2;
    int TIME_SLICE = 100;
    WaveFormat waveFormat;
    
    WpfPlot plot = new();
    private void AudioInit() 
    {
        waveFormat = new(sampleRate, bits, channels); 
        systemAudioCapture.WaveFormat = waveFormat;

        MMDeviceEnumerator deviceEnumerator = new();
        MMDevice? defaultSpeaker = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        audioMeterInformation = defaultSpeaker?.AudioMeterInformation;

        System.Timers.Timer audioTimer = new(TIME_SLICE);
        audioTimer.Elapsed += AudioTimer_Elapsed;
        audioTimer.Start();

        amplitudes = new double[sampleRate*TIME_SLICE/1000];
        plot.Plot.Add.Signal(amplitudes, sampleRate/1000);
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

    double[] amplitudes;
    private void SystemAudioCapture_DataAvailable(object? sender, WaveInEventArgs e)
    {
        byte[] bytes = e.Buffer.Take(e.BytesRecorded).ToArray();
        float[] samples = GetSamples(bytes);
        for(int i = 0; i < samples.Length; i++) amplitudes[i] = samples[i];
        plot.Plot.Axes.AutoScale();
        plot.Refresh();
    }
    
    // Samples are the amplitudes
    private float[] GetSamples(byte[] bytes)
    {
        int sampleWidth = 2 * channels;
        // total bytes recorded = samples * bytes per sample * channels
        // 16 bit = 2 bytes => bytes per sample
        int samplesCount = bytes.Length / sampleWidth;

        // each sample has an amplitude
        float[] samples = new float[samplesCount];
        
        // for simplicity choose the first channel
        for(int i = 0; i < samplesCount; i++)
        {
            // [ ..., 0x00, 0x10, ...]
            //        ^(i)  ^(i+1) 
            //            <-| shift to the left halfway (16/2 = 8 bits)
            short sample = (short)(bytes[i*sampleWidth] | bytes[i*sampleWidth+ 1] << 8);
            samples[i] = sample / 32768f;
        }

        return samples;
    }
}
