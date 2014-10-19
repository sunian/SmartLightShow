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
            // This stuff probably doesn't work but whatever.
            //WaveStream stream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(waveFileReader));
    
            //int bytesRecorded = 0;
            //int chunkSize = 4;
            //byte[] buffer = new byte[chunkSize];
            //do {
            //    bytesRecorded = stream.Read(buffer, 0, chunkSize);
            //    float sample32 = BitConverter.ToSingle(buffer, 0);
            //    sampleAggregator.Add(sample32);
            //} while (bytesRecorded != 0);

			BinaryReader reader = new BinaryReader(waveFileReader);

			int chunkID = reader.ReadInt32();
			int fileSize = reader.ReadInt32();
			int riffType = reader.ReadInt32();
			int fmtID = reader.ReadInt32();
			int fmtSize = reader.ReadInt32();
			int fmtCode = reader.ReadInt16();
			int channels = reader.ReadInt16();
			int sampleRate = reader.ReadInt32();
			int fmtAvgBPS = reader.ReadInt32();
			int fmtBlockAlign = reader.ReadInt16();
			int bitDepth = reader.ReadInt16();

			if (fmtSize == 18)
			{
				// Read any extra values
				int fmtExtraSize = reader.ReadInt16();
				reader.ReadBytes(fmtExtraSize);
			}

			int dataID = reader.ReadInt32();
			int dataSize = reader.ReadInt32();
			byte[] data = reader.ReadBytes(dataSize);

			int bytesPerSecond = bitDepth * sampleRate * channels / 8;

			Console.WriteLine(fmtBlockAlign * bitDepth + " " + dataSize + " " + data.Length);
			for (int i = 0; i < dataSize; i+=fmtBlockAlign)
			{
				float sample32 = BitConverter.ToSingle(data, i);
				sampleAggregator.Add(sample32);
			}

            Console.WriteLine("File analysis complete.");
            // Hold for now, remove this later obviously.
            while (true) {}
        }
    }
}
