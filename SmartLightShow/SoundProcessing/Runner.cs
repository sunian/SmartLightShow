﻿using SmartLightShow.SoundProcessing.Analyzers;
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
                Console.WriteLine("Input a filename: ");
                String filename = Console.ReadLine();
                RunFileAnalysis(filename);
            }
            else {
                RunMicAnalysis();
            }

        }

        public static void RunMicAnalysis() {
            Analyzer micAnalyzer = new MicAnalyzer();
            micAnalyzer.RunAnalysis();
        }

        public static void RunFileAnalysis(String filename) {
            Analyzer fileAnalyzer = new FileAnalyzer(filename);
            fileAnalyzer.RunAnalysis();
        }
    }
    // New FileAnalysis class.
    // Create a file reader, prompt for filename.
    // Read file with reader, get samples, add each to sample aggregator.
    // Then it does math
}