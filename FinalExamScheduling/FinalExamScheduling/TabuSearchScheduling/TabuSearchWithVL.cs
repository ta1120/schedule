using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalExamScheduling.Model;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TabuSearchWithVL : ITabuSearchAlgorithm
    {
        public Context ctx;

        TabuList globalTabuList;


        public TabuSearchWithVL(Context context)
        {
            ctx = context;
            globalTabuList = new TabuList();
        }

        public SolutionCandidate Start()
        {
            CandidateCostCalculator costCalc = new CandidateCostCalculator(ctx);
            NeighbourGeneratorWithVL neighbourGenerator = new NeighbourGeneratorWithVL(ctx);

            int examCount = ctx.Students.Length;

            bool NextModeIsRandom = false;

            bool TandemSearch = TSParameters.Mode.Equals("Tandem");

            SolutionCandidate current;
            SolutionCandidate bestSoFar;

            //kiindulo megoldas generalasa
            SolutionCandidate initialSolution = new RandomInitialSolutionGenerator(ctx).generateInitialSolution();
            current = initialSolution;
            bestSoFar = initialSolution;

            //koltseg es VL kiszamitasa
            current.Score = costCalc.Evaluate(current);
            Console.WriteLine("Initial schedule score: " + current.Score);
            current.VL = new ViolationList().Evaluate(current);

            //ciklus terminalasig
            int iterCounter = 1;
            int idleIterCounter = 0;
            int tandemIdleRunCounter = 0;
            double prevBestScore = current.Score;

            Console.WriteLine("Neighbour generation mode: " + TSParameters.Mode);
            if (TandemSearch) Console.WriteLine("Starting from " + (NextModeIsRandom ? "Random" : "Greedy") + " mode");

            while ((idleIterCounter < TSParameters.AllowedIdleIterations) || (TandemSearch && (tandemIdleRunCounter < TSParameters.TandemIdleSwitches)))
            {
                //szomszedok generalasa VL alapjan
                SolutionCandidate bestNeighbour = new SolutionCandidate(current.Clone().Schedule);
                SolutionCandidate aspirationCandidate = new SolutionCandidate(current.Clone().Schedule);

                int failedIterations = 0;

                //TODO: kivenni a módellenőrzést a cikluson kívülre + szépítés

                do
                {
                    SolutionCandidate[] neighbours = new SolutionCandidate[TSParameters.GeneratedCandidates];
                    switch (TSParameters.Mode)
                    {
                        case "Random":

                            if (failedIterations > TSParameters.MaxFailedNeighbourGenerations)
                            {
                                Console.WriteLine("Should terminate now...");
                                Console.WriteLine("If this keeps happening, maybe you should tweak the values in the TSParameters class.");
                                //globalTabuList.PrintTabuList();
                                return bestSoFar;
                            }
                            if (failedIterations > 0) Console.WriteLine("No feasible solutions were generated... Retrying #" + failedIterations);
                            neighbours = neighbourGenerator.GenerateNeighboursRandom(current.Clone());
                            //legjobb nem tabu szomszed kivalasztasa
                            bestNeighbour = SelectBestFeasibleCandidate(neighbours);
                            aspirationCandidate = SelectAspirationCandidate(neighbours);
                            failedIterations++;
                            break;
                        case "Greedy":
                            if (failedIterations > TSParameters.MaxFailedNeighbourGenerations)
                            {
                                Console.WriteLine("Should terminate now...");
                                Console.WriteLine("If this keeps happening, maybe you should tweak the values in the TSParameters class.");
                                //globalTabuList.PrintTabuList();
                                bestSoFar.VL.printViolations(bestSoFar);
                                return bestSoFar;
                            }
                            if (failedIterations > 0) Console.WriteLine("No feasible solutions were generated... Retrying #" + failedIterations);
                            neighbours = neighbourGenerator.GenerateNeighboursGreedy(current.Clone());
                            //legjobb nem tabu szomszed kivalasztasa
                            bestNeighbour = SelectBestFeasibleCandidate(neighbours);
                            aspirationCandidate = SelectAspirationCandidate(neighbours);
                            failedIterations++;
                            break;
                        case "Tandem":
                            if (idleIterCounter >= TSParameters.AllowedIdleIterations)
                            {
                                Console.WriteLine("\n[ Tandem runner change ]");
                                NextModeIsRandom = !NextModeIsRandom;
                                idleIterCounter = 0;
                                tandemIdleRunCounter++;
                            }
                            if(NextModeIsRandom)
                            {
                                if (failedIterations > TSParameters.MaxFailedNeighbourGenerations)
                                {
                                    NextModeIsRandom = false;
                                    failedIterations = 0;
                                    idleIterCounter = 0;
                                    tandemIdleRunCounter++;
                                    Console.WriteLine("\n[ Tandem runner change ]");
                                    //globalTabuList.PrintTabuList();
                                    //return bestSoFar;
                                }
                                if (failedIterations > 0) Console.WriteLine("[ Random ] No feasible solutions were generated... Retrying #" + failedIterations);

                                neighbours = neighbourGenerator.GenerateNeighboursRandom(current.Clone());
                                //legjobb nem tabu szomszed kivalasztasa
                                bestNeighbour = SelectBestFeasibleCandidate(neighbours);
                                aspirationCandidate = SelectAspirationCandidate(neighbours);
                                failedIterations++;
                            }
                            else 
                            {
                                if (failedIterations > TSParameters.MaxFailedNeighbourGenerations)
                                {
                                    NextModeIsRandom = true;
                                    failedIterations = 0;
                                    idleIterCounter = 0;
                                    tandemIdleRunCounter++;
                                    Console.WriteLine("\n[ Tandem runner change ]");
                                    //globalTabuList.PrintTabuList();
                                    //return bestSoFar;
                                }
                                if (failedIterations > 0) Console.WriteLine("[ Greedy ] No feasible solutions were generated... Retrying #" + failedIterations);
                                
                                neighbours = neighbourGenerator.GenerateNeighboursGreedy(current.Clone());
                                //legjobb nem tabu szomszed kivalasztasa
                                bestNeighbour = SelectBestFeasibleCandidate(neighbours);
                                aspirationCandidate = SelectAspirationCandidate(neighbours);
                                failedIterations++;
                            }
                            break;
                        default:
                            Console.WriteLine("TSParameters.Mode has an invalid value. It should be either Random/Greedy/Tandem");
                            return bestSoFar;
                            break;
                    }
                }
                while (bestNeighbour == null);

                if (aspirationCandidate != null)
                {
                    aspirationCandidate.VL = new ViolationList().Evaluate(aspirationCandidate);
                }

                //tabulista iteraciok csokkentese
                globalTabuList.DecreaseIterationsLeft();

                //tabulista kiegeszite
                ExpandTabuList(current, bestNeighbour);
                
                //jelenlegi beallitasa a legjobb szomszédra
                current = bestNeighbour;

                current.VL = new ViolationList().Evaluate(current);

                Console.WriteLine("#" + iterCounter++ + " iteration best neighbour score: " + current.Score);

                //legjobb megoldás frissítése ha jobb az új
                if (current.Score < bestSoFar.Score) bestSoFar = current;

                //AspirationCriteria ellenőrzése
                if (aspirationCandidate != null && bestSoFar.Score > aspirationCandidate.Score)
                {
                    Console.WriteLine("-> AspirationCriteria met, updating current and best schedule. Score: " + aspirationCandidate.Score);
                    current = aspirationCandidate.Clone();
                    bestSoFar = aspirationCandidate.Clone();
                    globalTabuList.tabuList.Clear();
                }

                //ellenőrizzük, hogy javult-e a legjobb megoldás
                if (bestSoFar.Score >= prevBestScore)
                {
                    idleIterCounter++;
                }
                else
                {
                    idleIterCounter = 0;
                    if (TandemSearch)
                    {
                        tandemIdleRunCounter = 0;
                        //Console.WriteLine("[ Improvement ] on this run.");
                    }
                }

                prevBestScore = bestSoFar.Score;
            }


            bestSoFar.VL = new ViolationList().Evaluate(bestSoFar);
            //Console.WriteLine("\nViolations in best found schedule: \n");
            bestSoFar.VL.printViolations(bestSoFar);
            
            //globalTabuList.PrintTabuList();

            if (idleIterCounter >= TSParameters.AllowedIdleIterations) Console.WriteLine("Maximum allowed idle iterations reached (" + TSParameters.AllowedIdleIterations + ")");

            return bestSoFar;
        }

        //Selects the best solution without tabu moves
        public SolutionCandidate SelectBestFeasibleCandidate(SolutionCandidate[] neighbours)
        {
            CandidateCostCalculator ccc = new CandidateCostCalculator(ctx);
            SolutionCandidate best = null;
            foreach(SolutionCandidate candidate in neighbours)
            {
                if (best == null && IsFeasibleSolution(candidate))
                {
                    candidate.Score = ccc.Evaluate(candidate);
                    best = candidate.Clone();
                    //Console.WriteLine("Score: " + candidate.Score);
                }
                else if(IsFeasibleSolution(candidate))
                {
                    candidate.Score = ccc.Evaluate(candidate);
                    if (candidate.Score < best.Score) best = candidate.Clone();
                    //Console.WriteLine("Score: " + candidate.Score);
                }
            }
            //Console.WriteLine("BestScore: " + best.Score);
            return best;
        }

        public SolutionCandidate SelectAspirationCandidate(SolutionCandidate[] neighbours)
        {
            CandidateCostCalculator ccc = new CandidateCostCalculator(ctx);
            SolutionCandidate best = null;
            foreach (SolutionCandidate candidate in neighbours)
            {
                if (best == null && !IsFeasibleSolution(candidate))
                {
                    candidate.Score = ccc.Evaluate(candidate);
                    best = candidate.Clone();
                    //Console.WriteLine("Score: " + candidate.Score);
                }
                else if (!IsFeasibleSolution(candidate))
                {
                    candidate.Score = ccc.Evaluate(candidate);
                    if (candidate.Score < best.Score) best = candidate.Clone();
                }
            }
            return best;
        }

        //Check whether a solution contains tabu elements/moves
        public bool IsFeasibleSolution(SolutionCandidate solution)
        {
            FinalExam[] exams = solution.Schedule.FinalExams;
            foreach(TabuListElement tabu in globalTabuList.GetTabuList())
            {
                if(tabu.TabuIterationsLeft > 0)
                {
                    switch (tabu.Attribute)
                    {
                        case "student":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if(i == tabu.ExamSlot)
                                {
                                    if (exams[i].Student.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "supervisor":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if (i == tabu.ExamSlot)
                                {
                                    if (exams[i].Supervisor.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "president":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if (i == tabu.ExamSlot)
                                {
                                    if (exams[i].President.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "secretary":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if (i == tabu.ExamSlot)
                                {
                                    if (exams[i].Secretary.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "member":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if (i == tabu.ExamSlot)
                                {
                                    if (exams[i].Member.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "examiner":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if (i == tabu.ExamSlot)
                                {
                                    if (exams[i].Examiner.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            return true;
        }

        //Add new elements to the tabu list based on changed attributes in schedules
        public void ExpandTabuList(SolutionCandidate current, SolutionCandidate bestNeighbour)
        {
            int examCount = ctx.Students.Length;
             
            for (int i = 0; i < examCount; i++)
            {
                Schedule oldSchedule = current.Schedule.Clone();
                Schedule newSchedule = bestNeighbour.Schedule.Clone();

                if (!oldSchedule.FinalExams[i].Student.Name.Equals(newSchedule.FinalExams[i].Student.Name))
                {
                    globalTabuList.Add(new TabuListElement("student", oldSchedule.FinalExams[i].Student.Name,i));
                }
                if (!oldSchedule.FinalExams[i].Supervisor.Name.Equals(newSchedule.FinalExams[i].Supervisor.Name))
                {
                    globalTabuList.Add(new TabuListElement("supervisor", oldSchedule.FinalExams[i].Supervisor.Name, i));
                }
                if (!oldSchedule.FinalExams[i].President.Name.Equals(newSchedule.FinalExams[i].President.Name))
                {
                    globalTabuList.Add(new TabuListElement("president", oldSchedule.FinalExams[i].President.Name, i));
                }
                if (!oldSchedule.FinalExams[i].Secretary.Name.Equals(newSchedule.FinalExams[i].Secretary.Name))
                {
                    globalTabuList.Add(new TabuListElement("secretary", oldSchedule.FinalExams[i].Secretary.Name, i));
                }
                if (!oldSchedule.FinalExams[i].Member.Name.Equals(newSchedule.FinalExams[i].Member.Name))
                {
                    globalTabuList.Add(new TabuListElement("member", oldSchedule.FinalExams[i].Member.Name, i));
                }
                if (!oldSchedule.FinalExams[i].Examiner.Name.Equals(newSchedule.FinalExams[i].Examiner.Name))
                {
                    globalTabuList.Add(new TabuListElement("examiner", oldSchedule.FinalExams[i].Examiner.Name, i));
                }
            }
        }
    }
}
