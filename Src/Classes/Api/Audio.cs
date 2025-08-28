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
using FftSharp;
using System.Numerics;
using System.Windows.Media;
using Colors = System.Windows.Media.Colors;
using System.Collections.Immutable;

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

    WaveFormat waveFormat = new(SAMPLE_RATE, BITS, CHANNELS);
    WpfPlot audioVisPlot;
    private void AudioInit() 
    {
        systemAudioCapture.WaveFormat = waveFormat;

        MMDeviceEnumerator deviceEnumerator = new();
        MMDevice? defaultSpeaker = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        audioMeterInformation = defaultSpeaker?.AudioMeterInformation;

        System.Timers.Timer audioTimer = new(TIME_SLICE);
        audioTimer.Elapsed += AudioTimer_Elapsed;
        audioTimer.Start();

        //for logging only
        //ThreadWindow threadWnd = new(
        //    init: wnd =>
        //    {
        //        wnd.WindowStyle = WindowStyle.None;
        //        wnd.AllowsTransparency = true;
        //        wnd.Background = new SolidColorBrush(Colors.Transparent);
        //    }
        //);
        ////plot = (WpfPlot?)_threadWnd.GetContent();
        //threadWnd.Run(() =>
        //{
        //    threadWnd.EnsureInitialized().wnd!.Content = audioVisPlot = new();

        //    audioVisPlot.Plot.FigureBackground = new() { Color = ScottPlot.Colors.Transparent };
        //    audioVisPlot.Plot.Axes.Color(ScottPlot.Colors.Transparent);
        //    audioVisPlot.Plot.Axes.FrameColor(ScottPlot.Colors.Transparent);
        //    audioVisPlot.Plot.Grid.LineColor = ScottPlot.Colors.Transparent;
        //});
        //Utils.HideWindowInAltTab(threadWnd.EnsureInitialized().hWnd);
        //CreateAudioVisualizer();
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
    
    // does not fire at fixed intervals which can be specified manually
    private void SystemAudioCapture_DataAvailable(object? sender, WaveInEventArgs e)
    {
        byte[] bytes = e.Buffer.Take(e.BytesRecorded).ToArray();
        float[] samples = GetSamples(bytes);
        double[] amplitudes = new double[samples.Length];
        Array.Copy(samples, amplitudes, amplitudes.Length);
        
        // fast fourier transfor for frequencies from amplitudes
        double[] zeroPaddedAmplitudes = Pad.ZeroPad(amplitudes);
        System.Numerics.Complex[] complexFrequencyDistribution = FFT.Forward(zeroPaddedAmplitudes);
        double[] frequencyWeights = FFT.Magnitude(complexFrequencyDistribution);

        // amplitude plot
        //plot.Plot.Axes.SetLimitsY(-1, 1);
        //plot.Plot.Axes.SetLimitsX(0, SAMPLES_IN_TIME_SLICE);
        //UpdateScottPlot(amplitudes, SAMPLE_RATE/1000);

        // frequency plot
        double signalPeriod = 1.0f / ((double)frequencyWeights.Length / SAMPLE_RATE);
        audioVisPlot?.Plot.Axes.SetLimitsY(0, 0.3);
        audioVisPlot?.Plot.Axes.SetLimitsX(0, 20000);
        //plot.Plot.Axes.AutoScale();
        UpdateScottPlot(frequencyWeights, signalPeriod);
    }

    bool firstRender = true;
    List<double> signalData = new();
    private void UpdateScottPlot(double[] signalData, double signalPeriod)
    {
        MorphListIntoArray<double>(signalData, this.signalData);
        if(firstRender)
        {
            firstRender = false;
            audioVisPlot?.Plot.Add.Signal(this.signalData, signalPeriod);
        }
        audioVisPlot?.Refresh();
    } 

    private static void MorphListIntoArray<T>(T[] array, List<T> list)
    {
        for(int i = 0; i < array.Length; i++)
        {
            if (i >= list.Count) list.Add(array[i]);
            else list[i] = array[i];
        }
        list.RemoveRange(array.Length - 1, list.Count - array.Length);
    }

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
