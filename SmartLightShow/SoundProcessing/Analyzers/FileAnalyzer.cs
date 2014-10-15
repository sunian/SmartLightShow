using NAudio.Wave;
using System;

namespace SmartLightShow.SoundProcessing.Analyzers {
    class FileAnalyzer : Analyzer {

        // Used to read in file.
        private WaveFileReader fileReader;

        public FileAnalyzer(WaveFileReader fileReader) : base() {
            this.fileReader = fileReader;
        }

        override public void RunAnalysis() {
            Console.WriteLine("You started file analysis");
            int bytesRecorded = 0;
            int bufferIncrement = 1024;
            byte[] buffer = new byte[bufferIncrement];

            do {
                bytesRecorded = fileReader.Read(buffer, 0, bufferIncrement);
                float sample32 = BitConverter.ToSingle(buffer, 0);
                sampleAggregator.Add(sample32);
            } while (bytesRecorded != 0);
        }
    }
}
