﻿using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
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
            int sampleRate = 0;

			using(WaveFileReader tempReader = new WaveFileReader(this.fileName)) {
				WaveStream ws = WaveFormatConversionStream.CreatePcmStream(tempReader);

				WaveStream blockAlignedStream = new BlockAlignReductionStream(ws);
				WaveChannel32 waveChannel = new WaveChannel32(blockAlignedStream);

                sampleRate = waveChannel.WaveFormat.SampleRate;
				fftProc = new FFTProcessor(0, 0, sampleRate, 16);
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

            Complex[] fftBuffer = sampleAggregator.getFftBuffer();
            Console.WriteLine("TESTING BEAT DETECTION!");
            List<Complex[]> filterBanked = BeatDetector.Filterbank(fftBuffer, sampleRate);
            BeatDetector.Smoothing(filterBanked);
            BeatDetector.DiffRect(filterBanked);
            int fundTempo = BeatDetector.CombFilter(filterBanked);
            Console.WriteLine("Fundamental tempo is: " + fundTempo);


			Console.WriteLine("File analysis complete.");
            // Hold for now, remove this later obviously.
            while (true) {}
        }
    }
}