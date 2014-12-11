using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SmartLightShow.FrequencyAnalysis;
using SmartLightShow.SoundProcessing;
using SmartLightShow.Communication;
using System.Threading;
using System.Collections;

namespace SmartLightShow.InputHandling {
    class FileAnalyzer : Analyzer {
        // Name of file to read from.
        private string fileName;
        static bool playbackStarted = false;
        Hashtable fftProcessors = new Hashtable();

        public FileAnalyzer(String fileName) : base() {
            this.fileName = fileName;
            playbackStarted = false;
        }

        override public void RunAnalysis() {
            Console.WriteLine("File analysis beginning.");
            List<string> files = new List<string>();
            if (fileName[fileName.Length - 1] == '/')
            {
                files.Add(fileName + "low_eq.wav");
                files.Add(fileName + "high_eq.wav");
            }
            else
            {
                files.Add(fileName);
            }
            foreach (string file in files)
            {
                var thread = new Thread(() => processFile(file));
                thread.Start();
            }

            //Complex[] fftBuffer = sampleAggregator.getFftBuffer();

            //int sampleSize = (int)(2.2*4096*2);
            //Complex[] middleSample = fftBuffer.Skip(fftBuffer.Length - sampleSize / 2).Take(sampleSize).ToArray();
            //List<Complex[]> processed = BeatDetector.Filterbank(middleSample);
            //BeatDetector.Smoothing(processed);
            //BeatDetector.DiffRect(processed);
            //int fundTempo = BeatDetector.CombFilter(processed);

            //Console.WriteLine("File analysis complete. Fundamental tempo is " + fundTempo + " BPM.");
            // Hold for now, remove this later obviously.
            //while (true) {}
        }

        protected void processFile(string file)
        {
            using (WaveFileReader reader = new WaveFileReader(file))
            {
                WaveStream ws = WaveFormatConversionStream.CreatePcmStream(reader);

                WaveStream blockAlignedStream = new BlockAlignReductionStream(ws);
                WaveChannel32 waveChannel = new WaveChannel32(blockAlignedStream);

                int sampleRate = waveChannel.WaveFormat.SampleRate;
                Console.WriteLine("Sample rate: " + sampleRate);
                Console.WriteLine("totaltime=" + reader.TotalTime + "   samplecount=" + reader.SampleCount + "   rate=" + (reader.SampleCount / reader.TotalTime.TotalSeconds));

                //FFTProcessor fftProc = new FFTProcessorFollowMelody(50, 260, 800, sampleRate, 16);
                FFTProcessor fftProc = new FFTProcessor(50, 400, sampleRate, 16);
                fftProcessors.Add(file, fftProc);

                long sampleCount = reader.Length / reader.BlockAlign;
                ISampleProvider getSamples = reader.ToSampleProvider();

                if (16 == reader.WaveFormat.BitsPerSample)
                {
                    Wave16ToFloatProvider provider = new Wave16ToFloatProvider(reader);
                    getSamples = provider.ToSampleProvider();
                }
                int n = fftProcessors.Count - 1;
                SampleAggregator sampleAggregator = new SampleAggregator(fftLength);
                sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(
                    (object sender, FftEventArgs e) => FftCalculated(e, file)
                    );
                sampleAggregator.PerformFFT = true;
                float[] buffer = new float[1000];
                int offset = 0;
                int readCount = 10;
                do
                {
                    readCount = getSamples.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < readCount; i++)
                    {
                        sampleAggregator.Add(buffer[i]);
                    }
                    offset += 1000;
                } while (readCount > 0);
            }
        }

        protected void FftCalculated(FftEventArgs e, string file)
        {
            lock (this)
            {
                if (!playbackStarted)
                {
                    SerialToMSP430.staticInstance.startTimer();
                    WaveStream mainOutputStream = null;
                    if (fileName[fileName.Length - 1] == '/')
                    {
                        mainOutputStream = new WaveFileReader(fileName + "orig.wav");
                    }
                    else
                    {
                        mainOutputStream = new WaveFileReader(fileName);
                    }

                    WaveChannel32 volumeStream = new WaveChannel32(mainOutputStream);
                    WaveOutEvent player = new WaveOutEvent();
                    player.Init(volumeStream);
                    player.Play();
                    playbackStarted = true;
                }
            }
            
            ((FFTProcessor)fftProcessors[file]).ProcessFFT(e.Result);
        }

    }
}