using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;

namespace SmartLightShow.SoundProcessing {
    // See "Beat This" beat detection algorithm for more info on what's being done here.
    class BeatDetector {

        // Frequencies to use to divide our signal into six bands.
        static int[] frequencies = new int[] { 200, 400, 800, 1600, 3200 };
        static int numPulses = 3;   // Number of pulses in the comb filter.
        static int minBPM = 60;     // Minimum BPM to sweep from in comb filter.
        static int maxBPM = 240;    // Maximum BPM to sweep to in comb filter.
        static int maxFreq = 4096;  // Maximum freuqency
        static double winLength = 0.4;     // Window length for Hanning window function.

        // Take in a time-domain FFT signal, split signal into 6 bands by frequency.
        // Return the inverse FFTs of each band.
        public static List<Complex[]> Filterbank(Complex[] fft, int sampleRate) {
            // We will split up our FFT into six bands based on frequency ranges. Create bands for:
            // 0-200Hz, 200-400Hz, 400-800Hz, 800-1600Hz, 1600-3200Hz, and 3200Hz onwards.
            List<Complex[]> fftBands = new List<Complex[]>();
            int toSkip = 0;    // The number of elements we've already processed in the array.
            int toTake = 0;    // The size of the next sub-array we are going to take.
            
            // Split up FFT into appropriate six bands.
            for (int i = 0; i < frequencies.Length + 1; ++i) {
                toSkip += toTake;
                if (i < frequencies.Length) {
                    toTake = frequencies[i] * fft.Length / sampleRate;
                } else {
                    toTake = frequencies.Length - toTake;
                }
                fftBands.Add(fft.Skip(toSkip).Take(toTake).ToArray());
            }

            // Perform inverse FFT on each band.
            foreach (Complex[] fftBand in fftBands) {
                FastFourierTransform.FFT(false /* forward */, (int)Math.Log(fftBand.Length), fftBand);
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
                    FastFourierTransform.FFT(true /* forward */, (int)Math.Log(fftBand.Length), fftBand);
                }

                // Convolve signal with the right half of a Hanning window of length 0.4 seconds.
                // In the frequency domain, just multiply.
                double hanningLength = winLength * 2 * maxFreq;
                float hanningMultiplier = (float) (Math.Pow(Math.Cos((i + 1) * Math.PI / hanningLength / 2), 2));

                for (int j = 0; j < fftBand.Length; ++j) {
                    fftBand[j].X = fftBand[j].X * hanningMultiplier;
                }

                // IFFT to time domain.
                if (fftBand.Length > 0) {
                    FastFourierTransform.FFT(false /* forward */, (int)Math.Log(fftBand.Length), fftBand);
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
                    }
                }
            }
        }


        public static int CombFilter(List<Complex[]> fftBands) {
            float maxEnergy = -1;
            int bestBPM = 0;

            for (int i = 0; i < fftBands.Count; ++i) {
                Complex[] fftBand = fftBands[i];
                
                // Create array to hold filtered signal, initialize to zeros.
                Complex[] filter = new Complex[(numPulses - 1) * 2 * maxFreq + 1];
                for (int j = 0; j < filter.Length; ++j) {
                    Complex c = new Complex();
                    c.X = 0;
                    c.Y = 0;
                    filter[j] = c;
                    
                }

                // FFT to frequency domain.
                if (fftBand.Length > 0) {
                    FastFourierTransform.FFT(true /* forward */, (int)Math.Log(fftBand.Length), fftBand);
                }

                for (int curBPM = minBPM; curBPM <= maxBPM; ++curBPM) {
                    float energy = 0;
                    float nstep = 120 / curBPM * maxFreq;

                    for (int a = 0; a < numPulses; ++a) {
                        filter[(int)(a * nstep)].X = 1;
                    }

                    // FFT filter into frequency domain.
                    if (filter.Length > 0) {
                        FastFourierTransform.FFT(true /* forward */, (int)Math.Log(filter.Length), filter);
                    }

                    // Calculate energy after convolution
                    for (int j = 0; j < fftBand.Length; ++j) {
                        energy += (float) (Math.Pow(Math.Abs(filter[j].X * fftBand[j].X), 2));
                    }

                    if (energy > maxEnergy) {
                        maxEnergy = energy;
                        bestBPM = curBPM;
                    }
                }
            }

            return bestBPM;
        }
    }
}
