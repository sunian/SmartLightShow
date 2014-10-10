﻿using NAudio.Wave;
using NAudio.Dsp;
using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
using System.Linq;

namespace SmartLightShow.SoundProcessing.Analyzers {
    class MicAnalyzer : Analyzer {
        static int run = 0;
        // Other inputs are also usable. Just look through the NAudio library.
        private IWaveIn waveIn;
        private static int fftLength = 8192; // NAudio fft wants powers of two!

        // There might be a sample aggregator in NAudio somewhere but I made a variation for my needs
        private Aggregator aggregator = new Aggregator(fftLength);

        public MicAnalyzer() {
            aggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            aggregator.PerformFFT = true;

            // Here you decide what you want to use as the waveIn.
            // There are many options in NAudio and you can use other streams/files.
            // Note that the code varies for each different source.
            MMDeviceEnumerator test = new MMDeviceEnumerator();
            waveIn = new WasapiCapture(test.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia));
            MMDeviceCollection all = test.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);
            Console.WriteLine("Sample Rate: " + waveIn.WaveFormat.SampleRate);

            waveIn.DataAvailable += OnDataAvailable;
        }

        void Analyzer.RunAnalysis() {
            try {
                waveIn.StartRecording();
            }
            catch (Exception e) {
                Debug.WriteLine(e.StackTrace);
            }
        }

        void OnDataAvailable(object sender, WaveInEventArgs e) {
            //if (this.InvokeRequired) {
            //    this.BeginInvoke(new EventHandler<WaveInEventArgs>(OnDataAvailable), sender, e);
            //}
            //else {
            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;
            int bufferIncrement = waveIn.WaveFormat.BlockAlign;

            for (int index = 0; index < bytesRecorded; index += bufferIncrement) {
                float sample32 = BitConverter.ToSingle(buffer, index);
                aggregator.Add(sample32);
            }
            //}
        }

        void FftCalculated(object sender, FftEventArgs e) {
            Console.WriteLine("Set#" + (++run));
            Debug.WriteLine("Received fft");
            int i = 0;
            Console.WriteLine("Result length: " + e.Result.Length);
            foreach (Complex c in e.Result) {
                if (Math.Sqrt(c.X * c.X + c.Y * c.Y) > 0.003) {
                    Console.WriteLine((i.ToString()) + "\tX:\t" + c.X + "\tY:\t" + c.Y + "\tMag:\t" + Math.Sqrt(c.X * c.X + c.Y * c.Y));
                }
                i++;
            }
        }
    }

}
