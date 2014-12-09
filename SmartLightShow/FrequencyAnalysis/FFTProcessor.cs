using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;
using SmartLightShow.Communication;

namespace SmartLightShow.FrequencyAnalysis {
	class FFTProcessor {
		protected double minFreq;
        protected double maxFreq;
        protected long sampleRate;
        protected int numBuckets;
        protected long fftCount;
        protected double[] pastMag1, pastMag2, pastMag3;
        protected double[] peakMags;

		static readonly double MIN_MAGNITUDE = 0.0035;

		public FFTProcessor(int min, int max, int sampleRate, int lightStreams) {
			minFreq = min;
			maxFreq = max;
			this.sampleRate = sampleRate;
			numBuckets = lightStreams;
            fftCount = 0;
			
		}

		public virtual bool[] ProcessFFT(Complex[] fft) {
            if (fftCount == 0) SerialToMSP430.staticInstance.startTimer();
            
			long fftLength = fft.Length;
            Console.WriteLine("fftlength = " + fftLength);
			bool[] lights = new bool[numBuckets];
            double[] mags = new double[fftLength];
            if (pastMag1 == null) pastMag1 = mags;
            if (pastMag2 == null) pastMag2 = pastMag1;
            if (pastMag3 == null) pastMag3 = pastMag2;
            if (peakMags == null) peakMags = mags;
            int lastBucket = -1; double maxMag = -1000;
            //Console.WriteLine(string.Join(" , ", lights));
			for (int i = 0; i < fftLength; i++) {
				double freq = ((double) i) / fftLength * sampleRate;
                if (freq > 800) break;
				double mag = Math.Sqrt(fft[i].X * fft[i].X + fft[i].Y * fft[i].Y);
				double phase = Math.Tan(fft[i].Y / fft[i].X);
                mags[i] = mag;
                int bucket = 0;
                bool showLight = false;
				if (mag > MIN_MAGNITUDE) {
					minFreq = Math.Max(Math.Min(minFreq, freq), 42);
					maxFreq = Math.Max(maxFreq, freq);
                    if (freq < minFreq) freq = minFreq;

                    double bucketStep = Math.Log(maxFreq / minFreq, 2) / numBuckets;
                    bucket = (int) (Math.Log(freq / minFreq, 2) / bucketStep);
                    if (bucket > 15) bucket = 15;
                    //double currentMin = (minFreq + maxFreq) / 2.0;
                    //while (freq < currentMin && bucket > 0) {
                    //    Console.WriteLine("min=" + currentMin);
                    //    bucket--;
                    //    currentMin = (currentMin + minFreq) / 2.0;
                    //}
                    //double cent = 1200 * (Math.Log(freq / 13.75, 2) - 0.25);
					Console.WriteLine(freq + " " + minFreq + " " + maxFreq + " " + bucket + " mag=" + (Math.Log(mag, 10) + 5));
                    if (peakMags[i] == 0)
                    {
                        if (mag >= (pastMag1[i] + pastMag2[i] + pastMag3[i]) * 1.0)
                        {
					        showLight = true;
                            peakMags[i] = mag;
                        }
                    }
                    else
                    {
                        if (mag >= peakMags[i] * 0.6)
                        {
                            showLight = true;
                            if (mag > peakMags[i]) peakMags[i] = mag;
                        }
                        else
                        {
                            peakMags[i] = 0;
                        }
                    }
                }
                if (showLight)
                {
                    if (mag > maxMag)
                    {
                        lastBucket = bucket;
                        maxMag = mag;
                    }
                }
                else
                {
                    if (lastBucket >= 0) lights[lastBucket] = true;
                    lastBucket = -1;
                    maxMag = -1000;
                }
			}
            if (lastBucket >= 0) lights[lastBucket] = true;
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
            Console.WriteLine("offset=" + fftCount * fftLength * 500L / sampleRate);
			SerialToMSP430.staticInstance.sendBytes(write, fftCount * fftLength * 500L / sampleRate);
            fftCount++;
            //Console.WriteLine(string.Join(" , ", write));
            pastMag3 = pastMag2;
            pastMag2 = pastMag1;
            pastMag1 = mags;
			return lights;
		}
	}
}
