using NAudio.Wave;
using System;
using System.IO;
using System.Text;
using SmartLightShow.Communication;

namespace SmartLightShow.InputHandling {
    public class Runner {
        public static void Main() {
            String choice = "";
            while (!choice.Equals("f", StringComparison.InvariantCultureIgnoreCase)
                    && !choice.Equals("m", StringComparison.InvariantCultureIgnoreCase)) {
                Console.WriteLine("Do you want (f)ile- or (m)icrophone-based analysis?");
                choice = Console.ReadLine();
            }
            SerialToMSP430.staticInstance.open();
            if (choice.Equals("f", StringComparison.InvariantCultureIgnoreCase)) {
                RunFileAnalysis();
            } else {
                RunMicAnalysis();
            }
            SerialToMSP430.staticInstance.close();
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
                if (fileName[fileName.Length - 1] == '/') break;
                if (File.Exists(fileName)) break;
                Console.WriteLine("File does not exist.");
            }
            while (true)
            {
                Analyzer fileAnalyzer = new FileAnalyzer(fileName);
                fileAnalyzer.RunAnalysis();
                String choice = Console.ReadLine();
                if (choice.Length > 0) break;
            }
        }
    }
}