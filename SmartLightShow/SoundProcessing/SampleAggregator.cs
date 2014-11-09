using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;

/*
 * Code used from various NAudio demos.
 */

namespace SmartLightShow.SoundProcessing {
    class SampleAggregator {
        // FFT
        public event EventHandler<FftEventArgs> FftCalculated;
        public bool PerformFFT { get; set; }

        // This fftBuffer is in frequency domain.
        private Complex[] fftBuffer;
        private FftEventArgs fftArgs;
        private int fftPos;
        private int fftLength;
        private int m;

        public SampleAggregator(int fftLength) {
            if (!IsPowerOfTwo(fftLength)) {
                throw new ArgumentException("FFT Length must be a power of two");
            }
            this.m = (int)Math.Log(fftLength, 2.0);
            this.fftLength = fftLength;
            this.fftBuffer = new Complex[fftLength];
            this.fftArgs = new FftEventArgs(fftBuffer);
        }

        // Helper method.
        bool IsPowerOfTwo(int x) {
            return (x & (x - 1)) == 0;
        }

        // Adds a float to the sampleAggregator.
        public void Add(float value) {
            if (PerformFFT && FftCalculated != null) {
                // Remember the window function! There are many others as well.
                fftBuffer[fftPos].X = (float)(value * FastFourierTransform.HammingWindow(fftPos, fftLength));
                fftBuffer[fftPos].Y = 0; // This is always zero with audio.
                fftPos++;
                if (fftPos >= fftLength) {
                    fftPos = 0;
                    FastFourierTransform.FFT(true /* forward */, m, fftBuffer);
                    FftCalculated(this, fftArgs);
                }
            }
        }

        public Complex[] getFftBuffer() {
            return fftBuffer;
        }
    }

    // Arguments capturing information about a FFT event.
    public class FftEventArgs : EventArgs {
        [DebuggerStepThrough]

        public FftEventArgs(Complex[] result) {
            this.Result = result;
        }
        
        // Public getter, private setter.
        public Complex[] Result { get; private set; }
    }
}