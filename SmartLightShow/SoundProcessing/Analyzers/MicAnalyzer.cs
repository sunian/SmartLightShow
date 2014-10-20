using NAudio.Wave;
using NAudio.Dsp;
using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
using System.Linq;

namespace SmartLightShow.SoundProcessing.Analyzers {
    class MicAnalyzer : Analyzer {
        // Used to read in microphone input.
        private IWaveIn waveIn;

        public MicAnalyzer() : base() {
            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
            waveIn = new WasapiCapture(deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia));
            MMDeviceCollection all = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);

            Console.WriteLine("Sample Rate: " + waveIn.WaveFormat.SampleRate);
            waveIn.DataAvailable += OnDataAvailable;
        }

        override public void RunAnalysis() {
            try {
                waveIn.StartRecording();
            }
            catch (Exception e) {
                Debug.WriteLine(e.StackTrace);
            }
        }

        void OnDataAvailable(object sender, WaveInEventArgs e) {
            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;
            int bufferIncrement = waveIn.WaveFormat.BlockAlign;

            for (int index = 0; index < bytesRecorded; index += bufferIncrement) {
                float sample32 = BitConverter.ToSingle(buffer, index);
                sampleAggregator.Add(sample32);
            }
        }
    }
}