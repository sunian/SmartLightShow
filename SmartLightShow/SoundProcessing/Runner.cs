using NAudio.Wave;
using SmartLightShow.SoundProcessing.Analyzers;
using System;
using System.IO;

namespace SmartLightShow.SoundProcessing {

    public class Runner {
        public static void Main() {
            String choice = "";
            while (!choice.Equals("f", StringComparison.InvariantCultureIgnoreCase)
                    && !choice.Equals("m", StringComparison.InvariantCultureIgnoreCase)) {
                Console.WriteLine("Do you want (f)ile- or (m)icrophone-based analysis?");
                choice = Console.ReadLine();
            }

            if (choice.Equals("f", StringComparison.InvariantCultureIgnoreCase)) {
                WaveFileReader fileReader = null;
                Console.WriteLine("Input a filename: ");
                while (fileReader == null) {
                    String filename = Console.ReadLine();
                    try {
                        fileReader = new WaveFileReader(filename);
                    }
                    catch (FileNotFoundException e) {
                        Console.WriteLine("Filename invalid. Input a filename: ");
                        fileReader = null;
                    }                   
                }

                RunFileAnalysis(fileReader);
            }
            else {
                RunMicAnalysis();
            }

        }

        public static void RunMicAnalysis() {
            Analyzer micAnalyzer = new MicAnalyzer();
            micAnalyzer.RunAnalysis();
        }

        public static void RunFileAnalysis(WaveFileReader fileReader) {
            Analyzer fileAnalyzer = new FileAnalyzer(fileReader);
            fileAnalyzer.RunAnalysis();
        }
    }
    // New FileAnalysis class.
    // Create a file reader, prompt for filename.
    // Read file with reader, get samples, add each to sample aggregator.
    // Then it does math
}
