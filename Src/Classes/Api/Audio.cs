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
using ScottPlot.Plottables;
using SkiaSharp;
using ScottPlot.DataSources;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace sambar;

public partial class Api
{
	WasapiLoopbackCapture systemAudioCapture = new();
	AudioMeterInformation? audioMeterInformation = null;

	const int SAMPLE_RATE = 44100;
	const int BITS = 16;
	const int CHANNELS = 2;
	const int SAMPLE_WIDTH = (BITS / sizeof(byte)) * CHANNELS;
	const int TIME_SLICE = 100;
	const int SAMPLES_IN_TIME_SLICE = SAMPLE_RATE * TIME_SLICE / 1000;
	const int BYTES_IN_TIME_SLICE = SAMPLES_IN_TIME_SLICE * SAMPLE_WIDTH * (BITS / sizeof(byte));

	WaveFormat waveFormat = new(SAMPLE_RATE, BITS, CHANNELS);
	// initialized in CreateAudioVisualizer()
	WpfPlot audioVisPlot;
	FilledSignal audioSignal;
	//Signal audioSignal;
	private void AudioInit()
	{
		systemAudioCapture.WaveFormat = waveFormat;

		MMDeviceEnumerator deviceEnumerator = new();
		MMDevice? defaultSpeaker = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
		audioMeterInformation = defaultSpeaker?.AudioMeterInformation;

		System.Timers.Timer audioTimer = new(TIME_SLICE);
		audioTimer.Elapsed += AudioTimer_Elapsed;
		audioTimer.Start();
	}

	private void AudioTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
	{
		if (audioMeterInformation?.MasterPeakValue > 0)
		{
			if (systemAudioCapture.CaptureState == CaptureState.Stopped)
			{
				systemAudioCapture.DataAvailable += SystemAudioCapture_DataAvailable;
				systemAudioCapture.StartRecording();
			}
			Logger.Log($"Audio Playing ... ");
		}
		else
		{
			if (systemAudioCapture.CaptureState == CaptureState.Capturing)
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
	// FIX_TODO: When audio is already playing when sambar starts, scottplot signal
	// plot appears shrunk
	bool firstRender = true;
	List<double> signalData = new();
	private void UpdateScottPlot(double[] signalData, double signalPeriod)
	{
		// WpfPlot holds a reference to "this.signalData", so just update it
		WriteArrayToListAndTrim<double>(signalData, this.signalData);
		if (firstRender)
		{
			firstRender = false;
			// if audio is playing while bar is launched this hits before
			// CreateAudioVisualizer() is called, if thats the case initialize
			// audio signal here
			if (audioSignal == null)
			{
				//audioSignal = audioVisPlot.Plot.Add.Signal(this.signalData, signalPeriod);
				audioSignal = FilledSignal.AddFilledSignalToPlot(audioVisPlot, this.signalData, signalPeriod);
			}
			else
			{
				audioSignal.Data.Period = signalPeriod;
			}
		}
		audioVisPlot?.Refresh();
	}

	private static void WriteArrayToListAndTrim<T>(T[] array, List<T> list)
	{
		for (int i = 0; i < array.Length; i++)
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
		for (int i = 0; i < samplesCount; i++)
		{
			// [ ..., 0x00, 0x10, ...]
			//        ^(i)  ^(i+1) 
			//            <-| shift to the left halfway (16/2 = 8 bits)
			short sample = (short)(bytes[i * SAMPLE_WIDTH] | bytes[i * SAMPLE_WIDTH + 1] << 8);
			samples[i] = sample / 32768f;
		}

		return samples;
	}
}

// custom ScottPlot.Plottables.Signal class that supports filling the insides of the 
// signal plot
public class FilledSignal : Signal
{
	public FilledSignal(ISignalSource data) : base(data) { }
	public override void Render(RenderPack rp)
	{
		if (!IsVisible) return;
		if (!Data.GetYs(MinRenderIndex, MaxRenderIndex).Any()) return;

		CoordinateRange visibleXRange = GetVisibleXRange(Axes.DataRect);
		int index = Data.GetIndex(visibleXRange.Min, clamp: true);
		int index2 = Data.GetIndex(visibleXRange.Max + Data.Period, clamp: true);
		List<Pixel> list = new List<Pixel>();
		for (int i = index; i <= index2; i++)
		{
			float pixelX = Axes.GetPixelX(Data.GetX(i));
			float pixelY = Axes.GetPixelY(Data.GetY(i) * Data.YScale + Data.YOffset);
			Pixel item = new Pixel(pixelX, pixelY);
			list.Add(item);
		}
		using SKPath sKPath = new SKPath();
		float magicYVal = Axes.YAxis.GetPixel(0, rp.DataRect); // magic yVal, idk why it works and a simple 0 doesnt ??
		sKPath.MoveTo(new SKPoint() { X = list[0].X, Y = magicYVal });
		foreach (Pixel item2 in list)
		{
			sKPath.LineTo(item2.ToSKPoint());
		}
		sKPath.LineTo(new SKPoint() { X = list.Last().X, Y = magicYVal });
		sKPath.Close();
		using SKPaint sKPaint = new SKPaint();
		LineStyle.ApplyToPaint(sKPaint);

		// draw fill
		FillStyle FillStyle = new() { IsVisible = true, Color = new(System.Drawing.Color.Blue) };
		PixelRect pixelRect = new();
		FillStyle.ApplyToPaint(sKPaint, pixelRect);
		rp.Canvas.DrawPath(sKPath, sKPaint);
	}

	private CoordinateRange GetVisibleXRange(PixelRect dataRect)
	{
		double coordinateX = Axes.GetCoordinateX(dataRect.Left);
		double coordinateX2 = Axes.GetCoordinateX(dataRect.Right);
		if (!(coordinateX <= coordinateX2))
		{
			return new CoordinateRange(coordinateX2, coordinateX);
		}
		return new CoordinateRange(coordinateX, coordinateX2);
	}

	public static FilledSignal AddFilledSignalToPlot(WpfPlot audioVisPlot, List<double> signalData, double? signalPeriod = null)
	{
		SignalSourceDouble signalSource;
		if (signalPeriod == null)
			signalSource = new(signalData, 1);
		else
			signalSource = new(signalData, (double)signalPeriod);
		FilledSignal audioSignal = new(signalSource);
		audioVisPlot.Plot.PlottableList.Add(audioSignal);
		return audioSignal;
	}
}
