using NAudio.Wave;
using System;
using System.IO;

namespace SmartLightShow.SoundProcessing.Analyzers {
    class FileAnalyzer : Analyzer {

        // Used to read in file.
        private string fileName;

        public FileAnalyzer(String fileName) : base() {
            this.fileName = fileName;
        }

        override public void RunAnalysis() {
            Console.WriteLine("You started file analysis");
			using(WaveFileReader reader = new WaveFileReader(this.fileName)) {
				long sampleCount = reader.Length / reader.BlockAlign;
				ISampleProvider getSamples = reader.ToSampleProvider();
				
				if (16 == reader.WaveFormat.BitsPerSample) {
					Wave16ToFloatProvider provider = new Wave16ToFloatProvider(reader);
					getSamples = provider.ToSampleProvider();
				}

				float[] buffer = new float[1000];
				int offset = 0;
				int rCount = 10;
				do {
					rCount = getSamples.Read(buffer, 0, 1000);
					for (int i = 0; i < rCount; i++ ) {
						sampleAggregator.Add(buffer[i]);
					}
					offset += 1000;
				} while (rCount > 0);
			}

			Console.WriteLine("File analysis complete.");
            // Hold for now, remove this later obviously.
            while (true) {}
        }
    }
}
