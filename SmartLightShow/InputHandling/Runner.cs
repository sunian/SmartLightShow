using NAudio.Wave;
using System;
using System.IO;
using System.Text;

namespace SmartLightShow.InputHandling {
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
            } else {
                RunMicAnalysis();
            }
        }

        // Runs sound analysis using the microphone as input.
        public static void RunMicAnalysis() {
            Analyzer micAnalyzer = new MicAnalyzer();
            micAnalyzer.RunAnalysis();
        }

        // Runs sound analysis using a WAV file as input.
        public static void RunFileAnalysis() {
            String fileName;
            String promptString = "Input a filename (cwd: " + System.Environment.CurrentDirectory + ")";
            while (true) {
                Console.WriteLine(promptString);
                fileName = Console.ReadLine();
                if (File.Exists(fileName)) break;
                Console.WriteLine("File does not exist.");
            }

            Analyzer fileAnalyzer = new FileAnalyzer(fileName);
            fileAnalyzer.RunAnalysis();
        }
    }
}