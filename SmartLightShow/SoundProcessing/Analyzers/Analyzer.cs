using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartLightShow.SoundProcessing.Analyzers {
    abstract class Analyzer {

        protected static int runNum = 0;

        // Must be a power of two.
        private static int fftLength = 8192;
        
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
        protected void FftCalculated(object sender, FftEventArgs e) {
            Console.WriteLine("Set#" + (++runNum));
            Debug.WriteLine("Received fft");
            int i = 0;
            Console.WriteLine("Result length: " + e.Result.Length);
            foreach (Complex c in e.Result) {
                if (Math.Sqrt(c.X * c.X + c.Y * c.Y) > 0.003) {
                    Console.WriteLine((i.ToString()) + "\tX:\t" + c.X + "\tY:\t" + c.Y + "\tMag:\t" + Math.Sqrt(c.X * c.X + c.Y * c.Y));
                }
                i++;
            }
        }
    }
}
