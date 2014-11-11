using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;

namespace SmartLightShow.FrequencyAnalysis {
    // See "Beat This" beat detection algorithm for more info on what's being done here.
    class BeatDetector {

        // Frequencies to use to divide our signal into six bands.
        static int[] frequencies = new int[] { 200, 400, 800, 1600, 3200 };
        static int numPulses = 3;   // Number of pulses in the comb filter.
        static int minBPM = 60;     // Minimum BPM to sweep from in comb filter.
        static int maxBPM = 240;    // Maximum BPM to sweep to in comb filter.
        static int maxFreq = 4096;  // Maximum freuqency
        static double winLength = 0.4;     // Window length for Hanning window function.

        // Take in a frequency-domain FFT signal, split signal into 6 bands by frequency.
        // Return the time-domain signal of each band.
        public static List<Complex[]> Filterbank(Complex[] fft) {
            // We will split up our FFT into six bands based on frequency ranges. Create bands for:
            // 0-200Hz, 200-400Hz, 400-800Hz, 800-1600Hz, 1600-3200Hz, and 3200Hz onwards.
            List<Complex[]> fftBands = new List<Complex[]>();
            int toSkip = 0;    // The number of elements we've already processed in the array.
            int toTake = 0;    // The size of the next sub-array we are going to take.
            
            // Split up FFT into appropriate six bands.
            for (int i = 0; i < frequencies.Length + 1; ++i) {
                toSkip += toTake;
                if (i < frequencies.Length) {
                    toTake = frequencies[i] * fft.Length / 2 / maxFreq;
                } else {
                    toTake = frequencies.Length - toTake;
                }
                fftBands.Add(fft.Skip(toSkip).Take(toTake).ToArray());
            }

            // Perform inverse FFT on each band.
            foreach (Complex[] fftBand in fftBands) {
                FastFourierTransform.FFT(false /* forward */, (int)Math.Log(fftBand.Length, 2.0), fftBand);
            }

            return fftBands;
        }

        // Take in a time-domain FFT signal split into 6 bands by frequency. Full-wave rectify each
        // band, take FFT of each, convolve each with Half Hanning window. Does calculations in place.
        public static void Smoothing(List<Complex[]> fftBands) {
            for (int i = 0; i < fftBands.Count; ++i) {
                Complex[] fftBand = fftBands[i];
                // Full-wave rectify the signal.
                for (int j = 0; j < fftBand.Length; ++j) {
                    Complex cur = fftBand[j];
                    if (cur.X < 0) {
                        cur.X = -cur.X;
                        fftBand[j] = cur;
                    }
                }

                // FFT to frequency domain.
                if (fftBand.Length > 0) {
                    FastFourierTransform.FFT(true /* forward */, (int)Math.Log(fftBand.Length, 2.0), fftBand);
                }

                double hanningLength = winLength * 2 * maxFreq;
                Complex[] hann = new Complex[(int)(hanningLength)];
                for (int j = 0; j < (int)(hanningLength); ++j) {
                    Complex c = new Complex();
                    c.X = (float)(Math.Pow(Math.Cos((j + 1) * Math.PI / hanningLength / 2), 2));
                    c.Y = 0;
                    hann[j] = c;
                }
                
                // FFT hanning Window to frequency domain.
                FastFourierTransform.FFT(true /* forward */, (int)Math.Log(hann.Length, 2.0), hann);

                // Convolve signal with the right half of a Hanning window of length 0.4 seconds.
                // In the frequency domain, just multiply hann with the fftBand.
                for (int j = 0; j < fftBand.Length; ++j) {
                    fftBand[j].X = fftBand[j].X * hann[j].X;
                }

                // IFFT to time domain.
                if (fftBand.Length > 0) {
                    FastFourierTransform.FFT(false /* forward */, (int)Math.Log(fftBand.Length, 2.0), fftBand);
                }
            }
        }

        // Differentiates the six frequency-banded signals in time, then half-wave rectifies them
        // so we only see increases in sound. Does all calculations in place.
        public static void DiffRect(List<Complex[]> fftBands) {
            for (int i = 0; i < fftBands.Count; ++i) {
                Complex[] fftBand = fftBands[i];

                // Differentiate the band in time.
                for (int j = fftBand.Length - 1; j > 0; --j) {
                    float diff = fftBand[j].X - fftBand[j - 1].X;
                    // Half-wave rectify band.
                    if (diff > 0) {
                        fftBand[j].X = diff;
                        fftBand[j].Y = 0;
                    } else {
                        fftBand[j].X = 0;
                        fftBand[j].Y = 0;
                    }
                }
            }
        }


        public static int CombFilter(List<Complex[]> fftBands) {
            float maxEnergy = -1;
            int bestBPM = 0;


            // FFT all to frequency domain.
            for (int i = 0; i < fftBands.Count; ++i) {
                Complex[] fftBand = fftBands[i];
                if (fftBand.Length > 0) {
                    FastFourierTransform.FFT(true /* forward */, (int)Math.Log(fftBand.Length, 2.0), fftBand);
                }
            }

            // Create array to hold filtered signal, initialize to zeros.
            Complex[] filter;

            for (int curBPM = minBPM; curBPM <= maxBPM; ++curBPM) {                
                float energy = 0;
                float nstep = (float)(120.0 / curBPM * maxFreq);

                // Clear old values.
                //for (int j = 0; j < filter.Length; ++j) {
                //    Complex c = new Complex();
                //    c.X = 0;
                //    c.Y = 0;
                //    filter[j] = c;
                //}
                filter = new Complex[(numPulses - 1) * 2 * maxFreq + 1];

                // Set new values.
                for (int a = 0; a < numPulses; ++a) {
                    filter[(int)(a * nstep)].X = 1;
                }

                // FFT filter into frequency domain.
                if (filter.Length > 0) {
                    FastFourierTransform.FFT(true /* forward */, (int)Math.Log(filter.Length, 2.0), filter);
                }

                for (int i = 0; i < fftBands.Count; ++i) {
                    Complex[] fftBand = fftBands[i];

                    // Calculate energy after convolution
                    for (int j = 0; j < fftBand.Length; ++j) {
                        for (int k = 0; k < filter.Length; ++k) {
                            energy += (filter[k].X * fftBand[j].X) * (filter[k].X * fftBand[j].X);
                        }
                    }
                }

                Console.WriteLine("Checking " + curBPM + " BPM. energy is: " + energy + " and maxEnergy is: " + maxEnergy);
                if (energy > maxEnergy) {
                    Console.WriteLine("Setting new best BPM to: " + curBPM);
                    maxEnergy = energy;
                    bestBPM = curBPM;
                }
            }

            return bestBPM;
        }
    }
}
