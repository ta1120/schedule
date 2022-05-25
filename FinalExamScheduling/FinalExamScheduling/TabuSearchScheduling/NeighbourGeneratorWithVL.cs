using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class NeighbourGeneratorWithVL
    {
        private Context ctx;

        public NeighbourGeneratorWithVL(Context context)
        {
            ctx = context;
        }

        public SolutionCandidate[] GenerateNeighboursGreedy(SolutionCandidate current)
        {
            int candidateCount = TSParameters.GeneratedCandidates;

            SolutionCandidate[] neighbours = new SolutionCandidate[candidateCount];

            ViolationList VL = current.VL;

            Random rand = new Random();

            for (int i = 0; i < candidateCount; i++)
            {
                neighbours[i] = new SolutionCandidate(current.Clone().Schedule);

                foreach (KeyValuePair<string, string> v in VL.Violations)
                {
                    if (v.Key.Equals("studentDuplicated") || v.Key.Equals("supervisorAvailability"))
                    {
                        int index = int.Parse(v.Value);
                        int x = rand.Next(0, ctx.Students.Length);
                        while (index == x && neighbours[i].Schedule.FinalExams[index].Student.Supervisor.Availability[x] && neighbours[i].Schedule.FinalExams[x].Student.Supervisor.Availability[index]) x = rand.Next(0, ctx.Students.Length);

                        Student temp = neighbours[i].Schedule.FinalExams[index].Student;

                        neighbours[i].Schedule.FinalExams[index].Student = neighbours[i].Schedule.FinalExams[x].Student;
                        neighbours[i].Schedule.FinalExams[x].Student = temp;
                        neighbours[i].Schedule.FinalExams[index].Supervisor = neighbours[i].Schedule.FinalExams[index].Student.Supervisor;
                        neighbours[i].Schedule.FinalExams[x].Supervisor = neighbours[i].Schedule.FinalExams[x].Student.Supervisor;
                    }
                    else if (v.Key.Equals("presidentAvailability"))
                    {

                        int x = rand.Next(0, ctx.Presidents.Length);
                        while (!ctx.Presidents[x].Availability[int.Parse(v.Value)])
                        {
                            x = rand.Next(0, ctx.Presidents.Length);
                        }
                        neighbours[i].Schedule.FinalExams[int.Parse(v.Value)].President = ctx.Presidents[x];

                    }
                    else if (v.Key.Equals("secretaryAvailability"))
                    {
                        int x = rand.Next(0, ctx.Secretaries.Length);
                        while (!ctx.Secretaries[x].Availability[int.Parse(v.Value)])
                        {
                            x = rand.Next(0, ctx.Secretaries.Length);
                        }
                        neighbours[i].Schedule.FinalExams[int.Parse(v.Value)].Secretary = ctx.Secretaries[x];
                    }
                    else if (v.Key.Equals("examinerAvailability") || v.Key.Equals("wrongExaminer"))
                    {
                        int x = rand.Next(0, ctx.Instructors.Length);
                        int ctr = 0;
                        int max = 10;
                        while (ctr < max && (!ctx.Instructors[x].Availability[int.Parse(v.Value)] || !neighbours[i].Schedule.FinalExams[int.Parse(v.Value)].Student.ExamCourse.Instructors.ToArray().Contains(ctx.Instructors[x])))
                        {
                            x = rand.Next(0, ctx.Instructors.Length);
                            ctr++;
                        }
                        // If no eligible examiner is available at the time, switch 2 random students and go on
                        if (ctr >= max)
                        {
                            int index = int.Parse(v.Value);
                            int y = rand.Next(0, ctx.Students.Length);
                            while (index == y) y = rand.Next(0, ctx.Students.Length);

                            Student temp = neighbours[i].Schedule.FinalExams[index].Student;

                            neighbours[i].Schedule.FinalExams[index].Student = neighbours[i].Schedule.FinalExams[y].Student;
                            neighbours[i].Schedule.FinalExams[y].Student = temp;
                            neighbours[i].Schedule.FinalExams[index].Supervisor = neighbours[i].Schedule.FinalExams[index].Student.Supervisor;
                            neighbours[i].Schedule.FinalExams[y].Supervisor = neighbours[i].Schedule.FinalExams[y].Student.Supervisor;
                        }
                        else neighbours[i].Schedule.FinalExams[int.Parse(v.Value)].Examiner = ctx.Instructors[x];
                    }
                    else if (v.Key.Equals("memberAvailability"))
                    {
                        int x = rand.Next(0, ctx.Members.Length);
                        while (!ctx.Members[x].Availability[int.Parse(v.Value)])
                        {
                            x = rand.Next(0, ctx.Members.Length);
                        }
                        neighbours[i].Schedule.FinalExams[int.Parse(v.Value)].Member = ctx.Members[x];
                    }

                    else if (v.Key.Equals("presidentChange"))
                    {
                        string[] indexes = v.Value.Split(';');
                        int feIndex = int.Parse(indexes[0]);
                        int offset = int.Parse(indexes[1]);

                        neighbours[i].Schedule.FinalExams[feIndex + offset].President = neighbours[i].Schedule.FinalExams[feIndex].President;
                    }
                    else if (v.Key.Equals("secretaryChange"))
                    {
                        string[] indexes = v.Value.Split(';');
                        int feIndex = int.Parse(indexes[0]);
                        int offset = int.Parse(indexes[1]);

                        neighbours[i].Schedule.FinalExams[feIndex].Secretary = neighbours[i].Schedule.FinalExams[feIndex + 1].Secretary;
                    }
                }

                //Trying to fix soft violations after
                if (TSParameters.OptimizeSoftConstraints)
                {
                    foreach (KeyValuePair<string, string> v in VL.Violations)
                    {
                        if (v.Key.Equals("supervisorNotPresident"))
                        {
                            int eI = int.Parse(v.Value);
                            int x = 0;
                            FinalExam[] fe = neighbours[i].Schedule.FinalExams;
                            while(x < ctx.Students.Length)
                            {
                                if(x != i && fe[x].President.Name.Equals(fe[eI].Supervisor.Name) && !fe[x].President.Name.Equals(fe[x].Supervisor.Name) && fe[x].President.Availability[x] && fe[x].Supervisor.Availability[eI] && fe[eI].Examiner.Availability[x] && fe[x].Examiner.Availability[eI])
                                {

                                    FinalExam tempI = fe[eI].Clone();
                                    FinalExam tempX = fe[x].Clone();
                                    fe[eI] = tempX.Clone();
                                    fe[x] = tempI.Clone();

                                    fe[eI].Secretary = tempI.Secretary;
                                    fe[eI].Member = tempI.Member;
                                    fe[x].President = tempX.President;
                                    fe[x].Secretary = tempX.Secretary;
                                    fe[x].Member = tempI.Member;
                                }

                                x++;
                            }
                        }
                        else if (v.Key.Equals("supervisorNotSecretary"))
                        {
                            int eI = int.Parse(v.Value);
                            int x = 0;
                            FinalExam[] fe = neighbours[i].Schedule.FinalExams;
                            while (x < ctx.Students.Length)
                            {
                                if (x != i && fe[x].Secretary.Name.Equals(fe[eI].Supervisor.Name) && !fe[x].Secretary.Name.Equals(fe[x].Supervisor.Name) && fe[x].Secretary.Availability[x] && fe[x].Supervisor.Availability[eI] && fe[eI].Examiner.Availability[x] && fe[x].Examiner.Availability[eI])
                                {

                                    FinalExam tempI = fe[eI].Clone();
                                    FinalExam tempX = fe[x].Clone();
                                    fe[eI] = tempX.Clone();
                                    fe[x] = tempI.Clone();

                                    fe[eI].President = tempI.President;
                                    fe[eI].Member = tempI.Member;
                                    fe[x].President = tempX.President;
                                    fe[x].Secretary = tempX.Secretary;
                                    fe[x].Member = tempI.Member;
                                }

                                x++;
                            }
                        }
                        else if (v.Key.Equals("examinerNotPresident"))
                        {
                            int eI = int.Parse(v.Value);
                            int x = 0;
                            FinalExam[] fe = neighbours[i].Schedule.FinalExams;
                            while (x < ctx.Students.Length)
                            {
                                if (x != i && fe[x].President.Name.Equals(fe[eI].Examiner.Name) && !fe[x].President.Name.Equals(fe[x].Examiner.Name) && fe[x].President.Availability[x] && fe[x].Examiner.Availability[eI])
                                {

                                    FinalExam tempI = fe[eI].Clone();
                                    FinalExam tempX = fe[x].Clone();
                                    fe[eI] = tempX.Clone();
                                    fe[x] = tempI.Clone();

                                    fe[eI].Secretary = tempI.Secretary;
                                    fe[eI].Member = tempI.Member;
                                    fe[x].President = tempX.President;
                                    fe[x].Secretary = tempX.Secretary;
                                    fe[x].Member = tempI.Member;
                                }

                                x++;
                            }
                        }/*
                        else if (v.Key.Equals("examinerNotSecretary"))
                        {
                            int eI = int.Parse(v.Value);
                            int x = 0;
                            FinalExam[] fe = neighbours[i].Schedule.FinalExams;
                            while (x < ctx.Students.Length)
                            {
                                if (x != i && fe[x].Secretary.Name.Equals(fe[eI].Examiner.Name) && !fe[x].Secretary.Name.Equals(fe[x].Examiner.Name) && fe[x].Secretary.Availability[x] && fe[x].Examiner.Availability[eI])
                                {

                                    FinalExam tempI = fe[eI].Clone();
                                    FinalExam tempX = fe[x].Clone();
                                    fe[eI] = tempX.Clone();
                                    fe[x] = tempI.Clone();

                                    fe[eI].President = tempI.President;
                                    fe[eI].Member = tempI.Member;
                                    fe[x].President = tempX.President;
                                    fe[x].Secretary = tempX.Secretary;
                                    fe[x].Member = tempI.Member;
                                }

                                x++;
                            }
                        }
                        else if (v.Key.Equals("examinerNotMember"))
                        {
                            int eI = int.Parse(v.Value);
                            int x = 0;
                            FinalExam[] fe = neighbours[i].Schedule.FinalExams;
                            while (x < ctx.Students.Length)
                            {
                                if (x != i && fe[x].Member.Name.Equals(fe[eI].Examiner.Name) && !fe[x].Member.Name.Equals(fe[x].Examiner.Name) && fe[x].Member.Availability[x] && fe[x].Examiner.Availability[eI])
                                {

                                    FinalExam tempI = fe[eI].Clone();
                                    FinalExam tempX = fe[x].Clone();
                                    fe[eI] = tempX.Clone();
                                    fe[x] = tempI.Clone();

                                    fe[eI].Secretary = tempI.Secretary;
                                    fe[eI].President = tempI.President;
                                    fe[x].President = tempX.President;
                                    fe[x].Secretary = tempX.Secretary;
                                    fe[x].Member = tempI.Member;
                                }

                                x++;
                            }
                        }
                        else if (v.Key.Equals("supervisorNotMember"))
                        {
                            int eI = int.Parse(v.Value);
                            int x = 0;
                            FinalExam[] fe = neighbours[i].Schedule.FinalExams;
                            while (x < ctx.Students.Length)
                            {
                                if (x != i && fe[x].Member.Name.Equals(fe[eI].Supervisor.Name) && !fe[x].Member.Name.Equals(fe[x].Supervisor.Name) && fe[x].Member.Availability[x] && fe[x].Supervisor.Availability[eI])
                                {

                                    FinalExam tempI = fe[eI].Clone();
                                    FinalExam tempX = fe[x].Clone();
                                    fe[eI] = tempX.Clone();
                                    fe[x] = tempI.Clone();

                                    fe[eI].Secretary = tempI.Secretary;
                                    fe[eI].President = tempI.President;
                                    fe[x].President = tempX.President;
                                    fe[x].Secretary = tempX.Secretary;
                                    fe[x].Member = tempI.Member;
                                }

                                x++;
                            }
                        }
                        else if (v.Key.Equals("supervisorNotExaminer"))
                        {
                            int eI = int.Parse(v.Value);
                            int x = 0;
                            FinalExam[] fe = neighbours[i].Schedule.FinalExams;
                            while (x < ctx.Students.Length)
                            {
                                if (x != i && fe[x].Supervisor.Name.Equals(fe[eI].Examiner.Name) && !fe[x].Supervisor.Name.Equals(fe[x].Examiner.Name) && fe[x].Supervisor.Availability[x] && fe[x].Examiner.Availability[eI])
                                {

                                    FinalExam tempI = fe[eI].Clone();
                                    FinalExam tempX = fe[x].Clone();
                                    fe[eI] = tempX.Clone();
                                    fe[x] = tempI.Clone();

                                    fe[eI].Secretary = tempI.Secretary;
                                    fe[eI].Member = tempI.Member;
                                    fe[eI].President = tempI.President;
                                    fe[x].President = tempX.President;
                                    fe[x].Secretary = tempX.Secretary;
                                    fe[x].Member = tempI.Member;
                                }

                                x++;
                            }
                        }*/
                    }
                }

            }
            return neighbours;
        }

        public SolutionCandidate[] GenerateNeighboursRandom(SolutionCandidate current)
        {
            int candidateCount = TSParameters.GeneratedCandidates;

            SolutionCandidate[] neighbours = new SolutionCandidate[candidateCount];

            ViolationList VL = current.VL;

            Random rand = new Random();

            for (int i = 0; i < candidateCount; i++)
            {
                neighbours[i] = new SolutionCandidate(current.Clone().Schedule);

                //Trying to fix soft violations first
                if (TSParameters.OptimizeSoftConstraints)
                {
                    foreach (KeyValuePair<string, string> v in VL.Violations)
                    {
                        if (v.Key.Equals("presidentWorkload"))
                        {
                            string[] data = v.Value.Split(';');
                            KeyValuePair<string, int> max = new KeyValuePair<string, int>(data[0], int.Parse(data[1]));
                            KeyValuePair<string, int> min = new KeyValuePair<string, int>(data[2], int.Parse(data[3]));
                            int optimalWorkload = ctx.Students.Length / ctx.Presidents.Length;
                            int maxToOptimal = max.Value - optimalWorkload;
                            int minToOptimal = optimalWorkload - min.Value;
                            int ctr = 0;
                            int target = 0;
                            if (maxToOptimal > 0 && maxToOptimal < minToOptimal)
                            {
                                target = maxToOptimal;
                            }
                            else if (minToOptimal > 0)
                            {
                                target = minToOptimal;
                            }
                            foreach (FinalExam fe in neighbours[i].Schedule.FinalExams)
                            {
                                if (ctr >= target) break;
                                if (fe.President.Name.Equals(max.Key))
                                {
                                    fe.President = ctx.GetInstructorByName(min.Key);
                                    ctr++;
                                }
                            }
                        }
                        else if (v.Key.Equals("secretaryWorkload"))
                        {
                            string[] data = v.Value.Split(';');
                            KeyValuePair<string, int> max = new KeyValuePair<string, int>(data[0], int.Parse(data[1]));
                            KeyValuePair<string, int> min = new KeyValuePair<string, int>(data[2], int.Parse(data[3]));
                            int optimalWorkload = ctx.Students.Length / ctx.Secretaries.Length;
                            int maxToOptimal = max.Value - optimalWorkload;
                            int minToOptimal = optimalWorkload - min.Value;
                            int ctr = 0;
                            int target = 0;
                            if (maxToOptimal > 0 && maxToOptimal < minToOptimal)
                            {
                                target = maxToOptimal;
                            }
                            else if (minToOptimal > 0)
                            {
                                target = minToOptimal;
                            }
                            foreach (FinalExam fe in neighbours[i].Schedule.FinalExams)
                            {
                                if (ctr >= target) break;
                                if (fe.Secretary.Name.Equals(max.Key))
                                {
                                    fe.Secretary = ctx.GetInstructorByName(min.Key);
                                    ctr++;
                                }
                            }
                        }
                        else if (v.Key.Equals("memberWorkload"))
                        {
                            string[] data = v.Value.Split(';');
                            KeyValuePair<string, int> max = new KeyValuePair<string, int>(data[0], int.Parse(data[1]));
                            KeyValuePair<string, int> min = new KeyValuePair<string, int>(data[2], int.Parse(data[3]));
                            int optimalWorkload = ctx.Students.Length / ctx.Members.Length;
                            int maxToOptimal = max.Value - optimalWorkload;
                            int minToOptimal = optimalWorkload - min.Value;
                            int ctr = 0;
                            int target = 0;
                            if (maxToOptimal > 0 && maxToOptimal < minToOptimal)
                            {
                                target = maxToOptimal;
                            }
                            else if (minToOptimal > 0)
                            {
                                target = minToOptimal;
                            }
                            foreach (FinalExam fe in neighbours[i].Schedule.FinalExams)
                            {
                                if (ctr >= target) break;
                                if (fe.Member.Name.Equals(max.Key))
                                {
                                    fe.Member = ctx.GetInstructorByName(min.Key);
                                    ctr++;
                                }
                            }
                        }
                    }
                }

                //Trying to fix hard violations after
                foreach (KeyValuePair<string, string> v in VL.Violations)
                {
                    if (v.Key.Equals("studentDuplicated") || v.Key.Equals("supervisorAvailability"))
                    {
                        int index = int.Parse(v.Value);
                        int x = rand.Next(0, ctx.Students.Length);
                        while (index == x) x = rand.Next(0, ctx.Students.Length);

                        Student temp = neighbours[i].Schedule.FinalExams[index].Student;

                        neighbours[i].Schedule.FinalExams[index].Student = neighbours[i].Schedule.FinalExams[x].Student;
                        neighbours[i].Schedule.FinalExams[x].Student = temp;
                        neighbours[i].Schedule.FinalExams[index].Supervisor = neighbours[i].Schedule.FinalExams[index].Student.Supervisor;
                        neighbours[i].Schedule.FinalExams[x].Supervisor = neighbours[i].Schedule.FinalExams[x].Student.Supervisor;
                        //Console.WriteLine("Swapped: " + neighbours[i].Schedule.FinalExams[index].Student.Name + " - " + neighbours[i].Schedule.FinalExams[x].Student.Name);
                    }
                    else if (v.Key.Equals("presidentAvailability"))
                    {

                        int x = rand.Next(0, ctx.Presidents.Length);

                        neighbours[i].Schedule.FinalExams[int.Parse(v.Value)].President = ctx.Presidents[x];

                    }
                    else if (v.Key.Equals("secretaryAvailability"))
                    {
                        int x = rand.Next(0, ctx.Secretaries.Length);

                        neighbours[i].Schedule.FinalExams[int.Parse(v.Value)].Secretary = ctx.Secretaries[x];
                    }
                    else if (v.Key.Equals("examinerAvailability") || v.Key.Equals("wrongExaminer"))
                    {
                        int x = rand.Next(0, ctx.Instructors.Length);

                        neighbours[i].Schedule.FinalExams[int.Parse(v.Value)].Examiner = ctx.Instructors[x];
                    }
                    else if (v.Key.Equals("memberAvailability"))
                    {
                        int x = rand.Next(0, ctx.Members.Length);

                        neighbours[i].Schedule.FinalExams[int.Parse(v.Value)].Member = ctx.Members[x];
                    }

                    else if (v.Key.Equals("presidentChange"))
                    {
                        int x = rand.Next(0, ctx.Presidents.Length);

                        string[] indexes = v.Value.Split(';');
                        int feIndex = int.Parse(indexes[0]);
                        int offset = int.Parse(indexes[1]);

                        neighbours[i].Schedule.FinalExams[feIndex + offset].President = ctx.Presidents[x];
                    }
                    else if (v.Key.Equals("secretaryChange"))
                    {
                        int x = rand.Next(0, ctx.Secretaries.Length);

                        neighbours[i].Schedule.FinalExams[int.Parse(v.Value.Split(';')[0])].Secretary = ctx.Secretaries[x];
                    }
                }
            }
            return neighbours;
        }
    }
}
