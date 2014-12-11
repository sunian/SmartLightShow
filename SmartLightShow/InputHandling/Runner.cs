using NAudio.Wave;
using System;
using System.IO;
using System.Text;
using SmartLightShow.Communication;
using System.Threading;

namespace SmartLightShow.InputHandling {
    public class Runner {
        public static void Main(string[] args) {
            barrier = new Barrier(2);
            SerialToMSP430.staticInstance.open();
            if (args.Length > 0)
            {
                RunFileAnalysis(args[0]);
            }
            else
            {
                String choice = "";
                while (!choice.Equals("f", StringComparison.InvariantCultureIgnoreCase)
                        && !choice.Equals("m", StringComparison.InvariantCultureIgnoreCase)) {
                    Console.WriteLine("Do you want (f)ile- or (m)icrophone-based analysis?");
                    choice = Console.ReadLine();
                }
                if (choice.Equals("f", StringComparison.InvariantCultureIgnoreCase)) {
                    RunFileAnalysis(null);
                } else {
                    RunMicAnalysis();
                }
            }
            SerialToMSP430.staticInstance.close();
        }

        // Runs sound analysis using the microphone as input.
        public static void RunMicAnalysis() {
            Analyzer micAnalyzer = new MicAnalyzer();
            micAnalyzer.RunAnalysis();
        }

        public static Barrier barrier;

        // Runs sound analysis using a WAV file as input.
        public static void RunFileAnalysis(String fileName)
        {
            String promptString = "Input a filename (cwd: " + System.Environment.CurrentDirectory + ")";
            while (true) {
                if (fileName != null) break;
                Console.WriteLine(promptString);
                fileName = Console.ReadLine();
                if (fileName[fileName.Length - 1] == '/') break;
                if (File.Exists(fileName)) break;
                Console.WriteLine("File does not exist.");
                fileName = null;
            }
            Analyzer fileAnalyzer = new FileAnalyzer(fileName);
            fileAnalyzer.RunAnalysis();
            Console.WriteLine("wa it");
            barrier.SignalAndWait();
            Console.WriteLine("do ne");
        }
    }
}