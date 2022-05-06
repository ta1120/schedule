using FinalExamScheduling.Model;
using FinalExamScheduling.TabuSearchScheduling;

using System;
using System.Diagnostics;
using System.IO;

namespace FinalExamScheduling
{
    public class Program
    {
        static TabuSearchScheduler scheduler;

        static void Main(string[] args)
        {
            RunTabuSearch();
            //RunGenetic();

            /*
            Console.WriteLine("Press Q to exit application.");
            ConsoleKeyInfo key;
            do {  key = Console.ReadKey();}
            while(key.Key == ConsoleKey.Q);
            System.Environment.Exit(0);
            */
        }

        //Based on the RunGenetic() function
        static void RunTabuSearch()
        {
            var watch = Stopwatch.StartNew();
            FileInfo existingFile = new FileInfo("Input.xlsx");

            var context = ExcelHelper.Read(existingFile);
            context.Init();
            scheduler = new TabuSearchScheduler(context);
            var task = scheduler.RunAsync().ContinueWith(scheduleTask =>
            {
                SolutionCandidate solution = scheduleTask.Result;
                Schedule resultSchedule = solution.Schedule;   
                double penaltyScore = solution.Score;
                string elapsed = watch.Elapsed.ToString();
                
                Console.WriteLine("Best penalty score: " + penaltyScore);

                string extraInfo = ("_" + TSParameters.Mode + "_" + penaltyScore);

                ExcelHelper.WriteTS(@"..\..\Results\Done_TS_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + extraInfo + ".xlsx", resultSchedule, context,  new CandidateCostCalculator(context).GetFinalScores(resultSchedule));
            }
            );

            while (true)
            {
                if (task.IsCompleted)
                    break;
                var ch = Console.ReadKey();
                if (ch.Key == ConsoleKey.A)
                {
                    scheduler.Cancel();
                }
                Console.WriteLine("Press A to Abort");
            }
            Console.WriteLine();
        }

        /*
        static void RunGenetic()
        {
            var watch = Stopwatch.StartNew();

            FileInfo existingFile = new FileInfo("Input.xlsx");

            var context = ExcelHelper.Read(existingFile);
            context.Init();
            scheduler = new GeneticScheduler(context);

            var task = scheduler.RunAsync().ContinueWith(scheduleTask =>
            {
                Schedule resultSchedule = scheduleTask.Result;   

                string elapsed = watch.Elapsed.ToString();

                SchedulingFitness evaluator = new SchedulingFitness(context);
                double penaltyScore = evaluator.EvaluateAll(resultSchedule);
                Console.WriteLine("Penalty score: " + penaltyScore);

                ExcelHelper.Write(@"..\..\Results\Done_Ge_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + penaltyScore + ".xlsx", scheduleTask.Result, elapsed, scheduler.GenerationFitness, scheduler.GetFinalScores(resultSchedule, scheduler.Fitness), context);

            }
            );

            while (true)
            {
                if (task.IsCompleted)
                    break;
                var ch = Console.ReadKey();
                if (ch.Key == ConsoleKey.A)
                {
                    scheduler.Cancel();
                }
                Console.WriteLine("Press A to Abort");
            }
            Console.WriteLine();
        }
        */
    }
}
