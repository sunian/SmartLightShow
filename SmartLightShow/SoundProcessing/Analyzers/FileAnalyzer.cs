using NAudio.Wave;
using System;

namespace SmartLightShow.SoundProcessing.Analyzers {
    class FileAnalyzer : Analyzer {

        private string filename;
        private WaveChannel32 wave;

        public FileAnalyzer(string filename) : base() {
            this.filename = filename;
            wave = new WaveChannel32(new WaveFileReader(this.filename));
        }

        override public void RunAnalysis() {
            Console.WriteLine("You started file analysis on " + filename);
        }
    }
}
