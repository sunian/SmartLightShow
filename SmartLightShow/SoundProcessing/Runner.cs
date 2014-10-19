using NAudio.Wave;
using SmartLightShow.SoundProcessing.Analyzers;
using System;

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
                RunFileAnalysis();
            }
            else {
                RunMicAnalysis();
            }
        }

        public static void RunMicAnalysis() {
            Analyzer micAnalyzer = new MicAnalyzer();
            micAnalyzer.RunAnalysis();
        }

        public static void RunFileAnalysis() {
            WaveFileReader fileReader = null;
            String promptString = "Input a filename (cwd: " + System.Environment.CurrentDirectory + ")";
            Console.WriteLine(promptString);
            while (fileReader == null) {
                String filename = Console.ReadLine();
                try {
                    fileReader = new WaveFileReader(filename);
                }
                catch (Exception e) {
                    Console.WriteLine("Filename invalid. " + promptString);
                    fileReader = null;
                }
            }

            Analyzer fileAnalyzer = new FileAnalyzer(fileReader);
            fileAnalyzer.RunAnalysis();
        }
    }
}
