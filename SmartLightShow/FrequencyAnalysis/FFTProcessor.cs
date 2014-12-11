using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;
using SmartLightShow.Communication;

namespace SmartLightShow.FrequencyAnalysis {
	class FFTProcessor {
		static protected double minFreq;
        static protected double maxFreq;
        protected long sampleRate;
        protected int numBuckets;
        protected long fftCount;
        protected double[] pastMag1, pastMag2, pastMag3;
        protected double[] peakMags;

        double MIN_MAGNITUDE = -38;

        double BASS_HIGH = 150;
        double MIN_BASS_MAGNITUDE = -20.5;

		public FFTProcessor(int min, int max, int sampleRate, int lightStreams) {
			minFreq = min;
			maxFreq = max;
			this.sampleRate = sampleRate;
			numBuckets = lightStreams;
            fftCount = 0;
			
		}
        double[] harmonicWeights = new double[] { 1.0, 1.0, 0.6, 0.4, 0.3, 0.3 , 0.2, 0.2, 0.2};
		public virtual bool[] ProcessFFT(Complex[] fft) {
            
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
                if (freq > 1000) break;
				double mag = Math.Sqrt(fft[i].X * fft[i].X + fft[i].Y * fft[i].Y);
                //if (freq >= BASS_HIGH)
                //{
                //    for (int j = 2; i * j < fftLength && j < 7; j++)
                //    {
                //        double harmag = Math.Sqrt(fft[i * j].X * fft[i * j].X + fft[i * j].Y * fft[i * j].Y) * harmonicWeights[j];
                //        if (Math.Log10(harmag) * 20 > MIN_MAGNITUDE * 1.17)
                //        {
                //            mag += harmag;
                //        }
                //    }
                //}
                mag = Math.Log10(mag) * 20;
				double phase = Math.Tan(fft[i].Y / fft[i].X);
                mags[i] = mag;
                int bucket = 0;
                bool showLight = false;
                //if (mag < BASS_HIGH && mag > MIN_BASS_MAGNITUDE)
                //{
                //    MIN_BASS_MAGNITUDE = mag;
                //}
                //if (mag >= BASS_HIGH && mag > MIN_MAGNITUDE)
                //{
                //    MIN_MAGNITUDE = mag;
                //}
                if ((mag > MIN_MAGNITUDE && freq >= BASS_HIGH) || (freq < BASS_HIGH && mag > MIN_BASS_MAGNITUDE)) {
					
                    //double currentMin = (minFreq + maxFreq) / 2.0;
                    //while (freq < currentMin && bucket > 0) {
                    //    Console.WriteLine("min=" + currentMin);
                    //    bucket--;
                    //    currentMin = (currentMin + minFreq) / 2.0;
                    //}
                    //double cent = 1200 * (Math.Log(freq / 13.75, 2) - 0.25);
					
                    if (peakMags[i] == -100)
                    {
                        if (mag >= (pastMag1[i] + pastMag2[i] + pastMag3[i]) * 0.7)
                        {
					        showLight = true;
                            peakMags[i] = mag;
                        }
                    }
                    else
                    {
                        if (mag >= peakMags[i] * 1.3)
                        {
                            showLight = true;
                            if (mag > peakMags[i]) peakMags[i] = mag;
                        }
                        else
                        {
                            peakMags[i] = -100;
                        }
                    }
                }
                if (showLight)
                {
                    minFreq = Math.Max(Math.Min(minFreq, freq), 42);
                    maxFreq = Math.Max(maxFreq, freq);
                    if (freq < minFreq) freq = minFreq;

                    double bucketStep = Math.Log(maxFreq / minFreq, 2) / numBuckets;
                    bucket = (int)(Math.Log(freq / minFreq, 2) / bucketStep);
                    if (bucket > 15) bucket = 15;
                    Console.WriteLine(i + "\t" + freq + "\t" + minFreq + "\t" + maxFreq + "\t" + bucket + "\tmag=" + mag + "\t" + (MIN_BASS_MAGNITUDE * 1.3));

                    if (mag > maxMag)
                    {
                        lastBucket = bucket;
                        maxMag = mag;
                    }
                }
                else
                {
                    if (lastBucket >= 0)
                    {
                        lights[lastBucket] = true;
                    }
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
