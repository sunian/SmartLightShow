using System;

namespace SmartLightShow.SoundProcessing.Analyzers {
    class FileAnalyzer : Analyzer {

        private string filename;

        public FileAnalyzer(string filename) {
            this.filename = filename;
        }

        override public void RunAnalysis() {
            Console.WriteLine("You started file analysis on " + filename);
        }
    }
}
