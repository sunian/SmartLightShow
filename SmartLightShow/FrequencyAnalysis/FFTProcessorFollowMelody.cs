using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;
using SmartLightShow.Communication;

namespace SmartLightShow.FrequencyAnalysis {
	class FFTProcessorFollowMelody : FFTProcessor {
        protected double maxMag;
        protected double maxTreble, minTreble, maxBass, minBass, maxSoprano, minSoprano;
        private List<double[]> fftSort;
        private double[][] rangeMappings = new double[][] { new double[]{5.999,10}, new double[]{7.999,0}, new double[]{1.999,8} };

		static readonly double MIN_MAGNITUDE = -36;

        public FFTProcessorFollowMelody(int min, int mid, int max, int sampleRate, int lightStreams)
            : base(min, max, sampleRate, lightStreams)
        {
            maxSoprano  = max * 1.2;
            minSoprano  = max / 1.2;
            maxTreble   = mid * 1.2;
            minTreble   = mid / 1.6;
            maxBass     = min * 1.0;
            minBass     = min / 1.2;
        }

		public override bool[] ProcessFFT(Complex[] fft) {
            if (fftCount == 0) SerialToMSP430.staticInstance.startTimer();
			long fftLength = fft.Length;
            fftSort = new List<double[]>();
            HashSet<int> freqs = new HashSet<int>();
            //Console.WriteLine("fftlength = " + fftLength);
			bool[] lights = new bool[numBuckets];
            double[] mags = new double[fftLength];
            if (pastMag1 == null) pastMag1 = mags;
            if (pastMag2 == null) pastMag2 = pastMag1;
            if (pastMag3 == null) pastMag3 = pastMag2;
            for (int i = 1; i < fftLength; i++)
            {
                double[] newFFT = new double[] {
                    ((double) i) / fftLength * sampleRate,//freq
                    20 * Math.Log10(Math.Sqrt(fft[i].X * fft[i].X + fft[i].Y * fft[i].Y)),//mag
                    Math.Tan(fft[i].Y / fft[i].X),//phase
                    i
                };
                if (newFFT[0] > 4000) break;
                if (newFFT[1] > MIN_MAGNITUDE)
                {
                    fftSort.Add(newFFT);
                    freqs.Add(i);
                }
            }
            fftSort.Sort((a, b) => Math.Sign(b[1] - a[1]) );
            if (fftSort.Count > 0)
            {
                double keyIndex = 0;
                for (int i = 0; i < Math.Min(8, fftSort.Count); i++)
                {
                    freqs.Add((int)fftSort[i][1]);
                    if (freqs.Contains((int)fftSort[i][1] - 1) || freqs.Contains((int)fftSort[i][1] + 1))
                    {
                        fftSort[i][1] = MIN_MAGNITUDE - 1;
                        continue;
                    }
                    
                    double freq = fftSort[i][0];
                    double mag = fftSort[i][1];
                    if (mag < -30 && i > 0 && mag < fftSort[i - 1][1] - 2.5) break;
                    if (freq / minTreble > maxBass / freq)
                    {
                        if (freq / minSoprano > maxTreble / freq)
                        {
                            minSoprano = Math.Min(minSoprano, freq);
                            maxSoprano = Math.Max(maxSoprano, freq);
                        }
                        else
                        {
                            minTreble = Math.Min(minTreble, freq);
                            maxTreble = Math.Max(maxTreble, freq);
                        }
                    }
                    else
                    {
                        minBass = Math.Min(minBass, freq);
                        maxBass = Math.Max(maxBass, freq);
                    }
                    //if (fftSort[i][1] / maxMag >= 0.34)
                    //{
                    //    maxMag = fftSort[i][1];
                    //    //Console.Write("freq = " + freq + "  ");
                    //    if (freq > maxFreq)
                    //    {
                    //        maxFreq = freq;
                    //        minFreq = freq / 2;
                    //    }
                    //    else if (freq < minFreq && freq >= minFreq / 4)
                    //    {
                    //        minFreq = freq;
                    //        maxFreq = freq * 2;
                    //    }
                    //}
                    //else
                    //{
                    //    maxMag *= 0.7;
                    //}
                }
                fftSort.Sort((a, b) => Math.Sign(b[1] - a[1]));
                maxMag = fftSort[0][1];
                List<double[]> ranges = new List<double[]>();
                ranges.Add(new double[]{0, minBass, maxBass});
                ranges.Add(new double[]{1, minTreble, maxTreble});
                ranges.Add(new double[]{2, minSoprano, maxSoprano});
                ranges.Sort((a, b) => Math.Sign(b[0] * 1.8 + b[2]/b[1] - a[2]/a[1] - a[0] * 1.8));
                for (int i = 0; i < ranges.Count; i++)
                {
                    Console.Write(ranges[i][0] + ": " + (int)ranges[i][1] + "," + (int)ranges[i][2] + "\t");
                }
                Console.WriteLine();
                //TODO determine light mapping for ranges
                //Console.WriteLine("\t\tmaxMag = " + maxMag);
                for (int i = 0; i < Math.Min(fftSort.Count, 16); i++)
                {
                    double freq = fftSort[i][0];
                    double mag = fftSort[i][1];
                    double phase = fftSort[i][2];
                    mags[i] = mag;
                    if (mag < MIN_MAGNITUDE) break;
                    if (mag < -30 && i > 0 && mag < fftSort[i - 1][1] - 2.5) break;
                    //if (freq < minFreq || freq > maxFreq) continue;
                    int bucket = 0;
                    double bucketStep;
                    bool inRange = false;
                    for (int r = 0; r < ranges.Count; r++)
                    {
                        if (freq >= ranges[r][1] && freq <= ranges[r][2])
                        {
                            inRange = true;
                            bucketStep = Math.Log(ranges[r][2] / ranges[r][1], 2) / rangeMappings[r][0];
                            bucket = (int)(Math.Log(freq / ranges[r][1], 2) / bucketStep);
                            bucket += (int)rangeMappings[r][1];
                            break;
                        }
                    }
                    if (!inRange) continue;
                    if (bucket < 0) bucket = 0;
                    if (bucket > 15) bucket = 15;
                    Console.WriteLine(fftSort[i][3] + "\t\t" + (int)freq + "\t" + "\t" + bucket + "\t\tmag=" + (int)mag);
                    //if (mag >= (pastMag1[i] + pastMag2[i] + pastMag3[i]) * 1.0) 
                    lights[bucket] = true;
                }
                maxSoprano = (10 * maxSoprano + 0.1 * minSoprano) / 10.1;
                minSoprano = (10 * minSoprano + 0.1 * maxSoprano) / 10.1;
                maxTreble = (10 * maxTreble + 0.1 * minTreble) / 10.1;
                minTreble = (10 * minTreble + 0.1 * maxTreble) / 10.1;
                maxBass = (10 * maxBass + 0.1 * minBass) / 10.1;
                minBass = (10 * minBass + 0.1 * maxBass) / 10.1;
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
