using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartLightShow.FrequencyAnalysis;
using SmartLightShow.SoundProcessing;


namespace SmartLightShow.InputHandling {
    abstract class Analyzer {

        // Used to work with the calculated FFT
		protected FFTProcessor fftProc;

        // Tracks the number of the current run.
        protected static int runNum = 0;

        // Must be a power of two.
        protected static int fftLength = 2048 * 1;
        
        // Variation of NAudio default sample aggregator.
        protected SampleAggregator sampleAggregator = new SampleAggregator(fftLength);

        // Base constructor, all inheriting classes should call this.
        public Analyzer() {
            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;
        }

        // Used externally to run analysis on any analyzer.
        abstract public void RunAnalysis();

        // Used internally to calculate an FFT.
        protected virtual void FftCalculated(object sender, FftEventArgs e) {
            Console.WriteLine();
            //Console.WriteLine("Set#" + (++runNum));
            //int i = 0;
            //Console.WriteLine("Result length: " + e.Result.Length);
            //foreach (Complex c in e.Result) {
            //    if (Math.Sqrt(c.X * c.X + c.Y * c.Y) > 0.005) {
            //        //Console.WriteLine((i * 48000 / 8192) + "\tX:\t" + c.X + "\tY:\t" + c.Y + "\tMag:\t" + Math.Sqrt(c.X * c.X + c.Y * c.Y));
            //    }
            //    i++;
            //}

			fftProc.ProcessFFT(e.Result);
        }
    }
}
