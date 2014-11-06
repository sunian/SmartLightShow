using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmartLightShow.SoundProcessing.Analyzers {
    class FileAnalyzer : Analyzer {
        // Name of file to read from.
        private string fileName;
        bool playbackStarted = false;

        public FileAnalyzer(String fileName) : base() {
            this.fileName = fileName;
        }

        override public void RunAnalysis() {
            Console.WriteLine("File analysis beginning.");
            int sampleRate = 0;

			using(WaveFileReader tempReader = new WaveFileReader(this.fileName)) {
				WaveStream ws = WaveFormatConversionStream.CreatePcmStream(tempReader);

				WaveStream blockAlignedStream = new BlockAlignReductionStream(ws);
				WaveChannel32 waveChannel = new WaveChannel32(blockAlignedStream);

                sampleRate = waveChannel.WaveFormat.SampleRate;
                Console.WriteLine("Sample rate: " + sampleRate);
				fftProc = new FFTProcessor(400, 4000, sampleRate, 16);
			}

			using(WaveFileReader reader = new WaveFileReader(this.fileName)) {
                Console.WriteLine("totaltime=" + reader.TotalTime + "   samplecount=" + reader.SampleCount + "   rate=" + (reader.SampleCount / reader.TotalTime.TotalSeconds));
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

            Complex[] fftBuffer = sampleAggregator.getFftBuffer();
            List<Complex[]> processed = BeatDetector.Filterbank(fftBuffer, sampleRate);
            BeatDetector.Smoothing(processed);
            BeatDetector.DiffRect(processed);
            int fundTempo = BeatDetector.CombFilter(processed);

			Console.WriteLine("File analysis complete.");
            // Hold for now, remove this later obviously.
            while (true) {}
        }

        protected override void FftCalculated(object sender, FftEventArgs e)
        {
            if (!playbackStarted)
            {
                WaveStream mainOutputStream = new WaveFileReader(fileName);
                WaveChannel32 volumeStream = new WaveChannel32(mainOutputStream);
                WaveOutEvent player = new WaveOutEvent();
                player.Init(volumeStream);
                player.Play();
                playbackStarted = true;
            }
            base.FftCalculated(sender, e);
        }
    }
}