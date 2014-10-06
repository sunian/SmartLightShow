using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Dsp;
using NAudio.CoreAudioApi;

namespace SmartLightShow.SoundProcessing {
    class MicAnalysis {
        // Other inputs are also usable. Just look through the NAudio library.
        private IWaveIn waveIn;
        private static int fftLength = 8192; // NAudio fft wants powers of two!

        // There might be a sample aggregator in NAudio somewhere but I made a variation for my needs
        private SampleAggregator sampleAggregator = new SampleAggregator(fftLength);

        public MicAnalysis() {
            Debug.WriteLine("Entered constructor");
            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;

            // Here you decide what you want to use as the waveIn.
            // There are many options in NAudio and you can use other streams/files.
            // Note that the code varies for each different source.
            MMDeviceEnumerator test = new MMDeviceEnumerator();
            waveIn = new WasapiCapture(test.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia));            
            MMDeviceCollection all = test.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);
            int n = 0;
            foreach (MMDevice dev in all) {
                Debug.WriteLine("Device " + n++ + ": " + dev);
            }

            waveIn.DataAvailable += OnDataAvailable;

            try {
                waveIn.StartRecording();
            }
            catch (Exception e) {
                Debug.WriteLine(e.StackTrace);
            }
            Debug.WriteLine("Left constructor");
        }

        public static void Main() {
            MicAnalysis main = new MicAnalysis();
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
                sampleAggregator.Add(sample32);
            }
            //}
        }

        void FftCalculated(object sender, FftEventArgs e) {
            Debug.WriteLine("Received fft");
            foreach (Complex c in e.Result) {
                if (c.X * c.Y != 0) {
                    Debug.WriteLine(c.X + " + " + c.Y + "j");
                    Console.WriteLine(c.X + " + " + c.Y + "j");
                }
            }
        }
    }
}
