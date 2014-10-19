using NAudio.Wave;
using System;
using System.IO;

namespace SmartLightShow.SoundProcessing.Analyzers {
    class FileAnalyzer : Analyzer {

        // Used to read in file.
        private WaveFileReader waveFileReader;

        public FileAnalyzer(WaveFileReader waveFileReader) : base() {
            this.waveFileReader = waveFileReader;
        }

        override public void RunAnalysis() {
            Console.WriteLine("You started file analysis");
            WaveStream stream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(waveFileReader));
    
            int bytesRecorded = 0;
            int chunkSize = 4;
            byte[] buffer = new byte[chunkSize];
            do {
                bytesRecorded = stream.Read(buffer, 0, chunkSize);
                float sample32 = BitConverter.ToSingle(buffer, 0);
                sampleAggregator.Add(sample32);
            } while (bytesRecorded != 0);

            Console.WriteLine("File analysis complete.");
            // Hold for now, remove this later obviously.
            while (true) {}
        }
    }
}
