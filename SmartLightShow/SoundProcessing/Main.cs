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
		static int run = 0;
        // Other inputs are also usable. Just look through the NAudio library.
        private IWaveIn waveIn;
        private static int fftLength = 8192; // NAudio fft wants powers of two!

        // There might be a sample aggregator in NAudio somewhere but I made a variation for my needs
        private SampleAggregator sampleAggregator = new SampleAggregator(fftLength);

        public MicAnalysis() {
            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;

            // Here you decide what you want to use as the waveIn.
            // There are many options in NAudio and you can use other streams/files.
            // Note that the code varies for each different source.
            MMDeviceEnumerator test = new MMDeviceEnumerator();
            waveIn = new WasapiCapture(test.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia));            
            MMDeviceCollection all = test.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);
			Console.WriteLine("Sample Rate: " + waveIn.WaveFormat.SampleRate);

            waveIn.DataAvailable += OnDataAvailable;

            try {
                waveIn.StartRecording();
            }
            catch (Exception e) {
                Debug.WriteLine(e.StackTrace);
            }
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
			Console.WriteLine("Set#" + (++run));
            Debug.WriteLine("Received fft");
			int i = 0;
			Console.WriteLine("Result length: " + e.Result.Length);
            foreach (Complex c in e.Result) {
				if (Math.Sqrt(c.X * c.X + c.Y * c.Y) > 0.003)
				{
					Console.WriteLine((i.ToString()) + "\tX:\t" + c.X + "\tY:\t" + c.Y + "\tMag:\t" + Math.Sqrt(c.X*c.X+c.Y*c.Y));
				}
				i++;
            }
        }
    }
}
