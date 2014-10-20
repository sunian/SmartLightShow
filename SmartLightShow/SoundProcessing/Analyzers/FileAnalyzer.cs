using NAudio.Wave;
using System;
using System.IO;

namespace SmartLightShow.SoundProcessing.Analyzers {
    class FileAnalyzer : Analyzer {
        // Name of file to read from.
        private string fileName;

        public FileAnalyzer(String fileName) : base() {
            this.fileName = fileName;
        }

        override public void RunAnalysis() {
            Console.WriteLine("File analysis beginning.");

			using(WaveFileReader tempReader = new WaveFileReader(this.fileName)) {
				WaveStream ws = WaveFormatConversionStream.CreatePcmStream(tempReader);

				WaveStream blockAlignedStream = new BlockAlignReductionStream(ws);
				WaveChannel32 waveChannel = new WaveChannel32(blockAlignedStream);
					
				fftProc = new FFTProcessor(0, 0, waveChannel.WaveFormat.SampleRate, 16);
			}

			using(WaveFileReader reader = new WaveFileReader(this.fileName)) {
				long sampleCount = reader.Length / reader.BlockAlign;
				ISampleProvider getSamples = reader.ToSampleProvider();
				
				if (16 == reader.WaveFormat.BitsPerSample) {
					Wave16ToFloatProvider provider = new Wave16ToFloatProvider(reader);
					getSamples = provider.ToSampleProvider();
				}

				float[] buffer = new float[1000];
				int offset = 0;
				int readCount = 10;
				do {
                    readCount = getSamples.Read(buffer, 0, 1000);
                    for (int i = 0; i < readCount; i++) {
						sampleAggregator.Add(buffer[i]);
					}
					offset += 1000;
                } while (readCount > 0);
			}

			Console.WriteLine("File analysis complete.");
            // Hold for now, remove this later obviously.
            while (true) {}
        }
    }
}