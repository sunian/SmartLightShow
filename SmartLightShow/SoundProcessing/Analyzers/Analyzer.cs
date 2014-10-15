using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartLightShow.SoundProcessing.Analyzers {
    abstract class Analyzer {

        protected static int run = 0;
        private static int fftLength = 8192; // NAudio fft wants powers of two!
        // There might be a sample aggregator in NAudio somewhere but I made a variation for my needs
        protected SampleAggregator sampleAggregator = new SampleAggregator(fftLength);

        // Default constructor, all inheriting classes should call this.
        public Analyzer() {
            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;
        }

        // Used externally to run analysis on any analyzer.
        abstract public void RunAnalysis();

        // Used internally to calculate an FFT.
        protected void FftCalculated(object sender, FftEventArgs e) {
            Console.WriteLine("Set#" + (++run));
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
