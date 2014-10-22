using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;
using SmartLightShow.Comm;

namespace SmartLightShow.SoundProcessing {
	class FFTProcessor {
		private int minFreq;
		private int maxFreq;
		private int sampleRate;
		private int numBuckets;
		private SerialToMSP430 serialComm;

		static readonly double MIN_MAGNITUDE = 0.003;

		public FFTProcessor(int min, int max, int sampleRate, int lightStreams) {
			minFreq = min;
			maxFreq = max;
			this.sampleRate = sampleRate;
			numBuckets = lightStreams;

			serialComm = new SerialToMSP430();
			serialComm.open();
		}

		public bool[] ProcessFFT(Complex[] fft) {
			int fftLength = fft.Length;
			bool[] lights = new bool[numBuckets];
            //Console.WriteLine(string.Join(" , ", lights));
			for (int i = 0; i < fftLength; i++) {
				double freq = ((double) i) / fftLength * sampleRate;
				double mag = Math.Sqrt(fft[i].X * fft[i].X + fft[i].Y * fft[i].Y);
				double phase = Math.Tan(fft[i].Y / fft[i].X);

				if (mag > MIN_MAGNITUDE) {
					minFreq = (int) Math.Min(minFreq, freq);
					maxFreq = (int) Math.Max(maxFreq, freq);

					double bucketSep = (maxFreq - minFreq) / (double)numBuckets;
					int bucket = 1;
					while (freq > bucket * bucketSep + minFreq && bucket < 16) {
						bucket++;
					}
					Console.WriteLine(freq + " " + minFreq + " " + maxFreq + " " + bucket);
					bucket--;
					lights[bucket] = true;
				}
			}
            //Console.WriteLine(string.Join(" , ", lights));
			byte[] write = new byte[2];
			byte now = 0;
			for (int i = 0; i < numBuckets; i++)
			{
				now |= (lights[i] ? (byte)(1<<(i%8)) : (byte)0);
				if (i % 8 == 7)
				{
					write[i / 8] = now;
                    //Console.WriteLine("now=" + now);
					now = 0;
				}
			}

			serialComm.sendBytes(write);
            //Console.WriteLine(string.Join(" , ", write));
			return lights;
		}
	}
}
