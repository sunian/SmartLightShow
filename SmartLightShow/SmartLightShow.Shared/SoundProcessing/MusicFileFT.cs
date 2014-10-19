﻿using System;
using System.Collections.Generic;
using System.Text;
using NAudio;
using NAudio.Codecs;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Dsp;
using NAudio.SoundFont;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Win8;
using NAudio.Win8.Wave;
using NAudio.Win8.Wave.WaveOutputs;

namespace SmartLightShow.SoundProcessing
{
    class MusicFileFT
    {
        private IWaveIn waveIn;
        private static int fftLength = 8192;

        private SampleAggregator sampleAggregator = new SampleAggregator(fftLength);

        public MusicFileFT()
        {
            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;
            // Here you decide what you want to use as the waveIn.
            // There are many options in NAudio and you can use other streams/files.
            // Note that the code varies for each different source.
            waveIn = new NAudio.Wave.Was();

            waveIn.DataAvailable += OnDataAvailable;

            waveIn.StartRecording();
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
{
    if (this.InvokeRequired)
    {
        this.BeginInvoke(new EventHandler<WaveInEventArgs>(OnDataAvailable), sender, e);
    }
    else
    {
        byte[] buffer = e.Buffer;
        int bytesRecorded = e.BytesRecorded;
        int bufferIncrement = waveIn.WaveFormat.BlockAlign;

        for (int index = 0; index < bytesRecorded; index += bufferIncrement)
        {
            float sample32 = BitConverter.ToSingle(buffer, index);
            sampleAggregator.Add(sample32);
        }
    }
}

        void FftCalculated(object sender, FftEventArgs e)
        {
            // Do something with e.result!
        }

    }
}
