using NAudio.Dsp;
using SmartLightShow.Comm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartLightShow.SoundProcessing.Analyzers {
    abstract class Analyzer {

        // Used to communicate with the MSP430.
        protected SerialToMSP430 serialComm;

        // Tracks the number of the current run.
        protected static int runNum = 0;

        // Must be a power of two.
        private static int fftLength = 8192;
        
        // Variation of NAudio default sample aggregator.
        protected SampleAggregator sampleAggregator = new SampleAggregator(fftLength);

        // Base constructor, all inheriting classes should call this.
        public Analyzer() {
            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;
            serialComm = new SerialToMSP430();
            serialComm.open();
        }

        // Used externally to run analysis on any analyzer.
        abstract public void RunAnalysis();

        // Used internally to calculate an FFT.
        protected void FftCalculated(object sender, FftEventArgs e) {
            Console.WriteLine("Set#" + (++runNum));
            int i = 0;
            Console.WriteLine("Result length: " + e.Result.Length);
            foreach (Complex c in e.Result) {
                if (Math.Sqrt(c.X * c.X + c.Y * c.Y) > 0.005) {
                    Console.WriteLine((i * 48000 / 8192) + "\tX:\t" + c.X + "\tY:\t" + c.Y + "\tMag:\t" + Math.Sqrt(c.X * c.X + c.Y * c.Y));
                }
                i++;
            }
        }
    }
}
