using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;
using SmartLightShow.Communication;

namespace SmartLightShow.FrequencyAnalysis {
	class FFTProcessorFollowMelody {
		private double minFreq;
		private double maxFreq;
        private double maxMag = MIN_MAGNITUDE;
		private long sampleRate;
		private int numBuckets;
		private SerialToMSP430 serialComm;
        private long fftCount;
        private double[] pastMag1, pastMag2, pastMag3;
        private List<double[]> fftSort;

		static readonly double MIN_MAGNITUDE = 0.0035;

        public FFTProcessorFollowMelody(int min, int max, int sampleRate, int lightStreams)
        {
			minFreq = min;
			maxFreq = max;
			this.sampleRate = sampleRate;
			numBuckets = lightStreams;
            fftCount = 0;
			
            serialComm = new SerialToMSP430();
			serialComm.open();
		}

		public bool[] ProcessFFT(Complex[] fft) {
            if (fftCount == 0) serialComm.startTimer();
			long fftLength = fft.Length;
            fftSort = new List<double[]>();
            Console.WriteLine("fftlength = " + fftLength);
			bool[] lights = new bool[numBuckets];
            double[] mags = new double[fftLength];
            if (pastMag1 == null) pastMag1 = mags;
            if (pastMag2 == null) pastMag2 = pastMag1;
            if (pastMag3 == null) pastMag3 = pastMag2;
            for (int i = 1; i < fftLength; i++)
            {
                double[] newFFT = new double[] {
                    ((double) i) / fftLength * sampleRate,//freq
                    Math.Sqrt(fft[i].X * fft[i].X + fft[i].Y * fft[i].Y),//mag
                    Math.Tan(fft[i].Y / fft[i].X)//phase
                };
                if (newFFT[0] > 4000) break;
                fftSort.Add(newFFT);
            }
            fftSort.Sort((a, b) => Math.Sign(b[1] - a[1]) );
            for (int i = 0; i >= 0; i--)
            {
                if (fftSort[i][1] / maxMag >= 0.34)
                {
                    maxMag = fftSort[i][1];
                    double freq = fftSort[i][0];
                    Console.Write("freq = " + freq + "  ");
                    if (freq > maxFreq)
                    {
                        maxFreq = freq;
                        minFreq = freq / 2;
                    }
                    else if (freq < minFreq && freq >= minFreq / 4)
                    {
                        minFreq = freq;
                        maxFreq = freq * 2;
                    }
                }
                else
                {
                    maxMag *= 0.7;
                }
            }
            Console.WriteLine("maxMag = " + maxMag);
			for (int i = 0; i < Math.Min(fftSort.Count, 3); i++) {
				double freq = fftSort[i][0];
                //if (freq > 4000) break;
                double mag = fftSort[i][1];
				double phase = fftSort[i][2];
                mags[i] = mag;
				if (mag > MIN_MAGNITUDE) {
                    //minFreq = Math.Max(Math.Min(minFreq, freq), 42);
                    //maxFreq = Math.Max(maxFreq, freq);
                    if (freq < minFreq || freq > maxFreq) continue;

                    double bucketStep = Math.Log(maxFreq / minFreq, 2) / 8;
                    int bucket = 8 + (int) (Math.Log(freq / minFreq, 2) / bucketStep);
                    if (bucket > 15) bucket = 15;
                    //double currentMin = (minFreq + maxFreq) / 2.0;
                    //while (freq < currentMin && bucket > 0) {
                    //    Console.WriteLine("min=" + currentMin);
                    //    bucket--;
                    //    currentMin = (currentMin + minFreq) / 2.0;
                    //}
                    //double cent = 1200 * (Math.Log(freq / 13.75, 2) - 0.25);
					Console.WriteLine(freq + " " + minFreq + " " + maxFreq + " " + bucket + " mag=" + mag);
                    //if (mag >= (pastMag1[i] + pastMag2[i] + pastMag3[i]) * 1.0) 
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
            //Console.WriteLine("offset=" + fftCount * fftLength * 500L / sampleRate);
			serialComm.sendBytes(write, fftCount * fftLength * 500L / sampleRate);
            fftCount++;
            //Console.WriteLine(string.Join(" , ", write));
            pastMag3 = pastMag2;
            pastMag2 = pastMag1;
            pastMag1 = mags;
			return lights;
		}
	}
}
