using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalExamScheduling.Model;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class ViolationList
    {
        public List<KeyValuePair<string, string>> Violations { get; set; }

        //private Context ctx;

        public List<Func<Schedule, ViolationList>> ViolationFunctions;

        public ViolationList()
        {
            Violations = new List<KeyValuePair<string, string>>();
        }

        public void AddViolation(string param, string value)
        {
            Violations.Add(new KeyValuePair<string,string>(param,value));
        }

        public void AddViolation(KeyValuePair<string, string> v)
        {
            Violations.Add(v);
        }

        public void printViolations(SolutionCandidate s)
        {
            if (s.VL == null) return;

            foreach(KeyValuePair<string, string> v in s.VL.Violations)
            {
                Console.WriteLine("Violation: " + v.Key + " - " + v.Value);
            }
        }

        public ViolationList Evaluate(SolutionCandidate cand)
        {
            Schedule sch = cand.Schedule;

            ViolationList vi = new ViolationList();

            ViolationFunctions = new List<Func<Schedule, ViolationList>>()
            {
                GetWrongExaminerViolations,
                GetStudentDuplicatedViolations,
                GetPresidentNotAvailableViolations,
                GetSecretaryNotAvailableViolations,
                GetExaminerNotAvailableViolations,
                GetMemberNotAvailableViolations,
                GetSupervisorNotAvailableViolations,

                GetPresidentChangeViolations,
                GetSecretaryChangeViolations
                /*
                GetPresidentWorkloadWorstScore,
                GetPresidentWorkloadWorseScore,
                GetPresidentWorkloadBadScore,
                GetSecretaryWorkloadWorstScore,
                GetSecretaryWorkloadWorseScore,
                GetSecretaryWorkloadBadScore,
                GetMemberWorkloadWorstScore,
                GetMemberWorkloadWorseScore,
                GetMemberWorkloadBadScore,

                GetPresidentSelfStudentScore,
                GetSecretarySelfStudentScore,
                GetExaminerNotPresidentScore
                */
           };

            var tasks = ViolationFunctions.Select(cf => Task.Run(() => cf(sch))).ToArray();
            Task.WaitAll(tasks);
            foreach (var task in tasks)
            {
                ViolationList viRes = task.Result;
                foreach(KeyValuePair<string, string> v in viRes.Violations)
                {
                    vi.AddViolation(v);
                }
            }

            return vi;
        }

        public ViolationList GetWrongExaminerViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for(int i = 0; i < sch.FinalExams.Length;i++)
            {
                if (!sch.FinalExams[i].Student.ExamCourse.Instructors.ToArray().Contains(sch.FinalExams[i].Examiner)) vl.AddViolation("wrongExaminer",i.ToString());
            }
            return vl;
        }

        public ViolationList GetStudentDuplicatedViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            List<Student> studentBefore = new List<Student>();
            int[] count = new int[100];
            foreach (var fe in sch.FinalExams)
            {
                count[fe.Student.Id]++;
            }
            for (int i = 0; i < 100; i++)
            {
                if (count[i] > 1)
                {
                    vl.AddViolation("studentDuplicated", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetPresidentNotAvailableViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].President.Availability[i] == false)
                {
                    vl.AddViolation("presidentAvailability", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetSecretaryNotAvailableViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Secretary.Availability[i] == false)
                {
                    vl.AddViolation("secretaryAvailability", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetExaminerNotAvailableViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Examiner.Availability[i] == false)
                {
                    vl.AddViolation("examinerAvailability", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetMemberNotAvailableViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Member.Availability[i] == false)
                {
                    vl.AddViolation("memberAvailability", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetSupervisorNotAvailableViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Supervisor.Availability[i] == false)
                {
                    vl.AddViolation("supervisorAvailability", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetPresidentChangeViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i += 5)
            {
                if (sch.FinalExams[i].President != sch.FinalExams[i + 1].President)
                {
                    vl.AddViolation("presidentChange", i.ToString() + ";" + "1");
                }
                if (sch.FinalExams[i].President != sch.FinalExams[i + 2].President)
                {
                    vl.AddViolation("presidentChange", i.ToString() + ";" + "2");
                }
                if (sch.FinalExams[i].President != sch.FinalExams[i + 3].President)
                {
                    vl.AddViolation("presidentChange", i.ToString() + ";" + "3");
                }
                if (sch.FinalExams[i].President != sch.FinalExams[i + 4].President)
                {
                    vl.AddViolation("presidentChange", i.ToString() + ";" + "4");
                }
            }
            return vl;
        }

        public ViolationList GetSecretaryChangeViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i += 5)
            {
                if (sch.FinalExams[i].Secretary != sch.FinalExams[i + 1].Secretary)
                {
                    vl.AddViolation("secretaryChange", i.ToString() + ";" + "1");
                }
                if (sch.FinalExams[i + 1].Secretary != sch.FinalExams[i + 2].Secretary)
                {
                    vl.AddViolation("secretaryChange", (i+1).ToString() + ";" + "2");
                }
                if (sch.FinalExams[i + 2].Secretary != sch.FinalExams[i + 3].Secretary)
                {
                    vl.AddViolation("secretaryChange", (i+2).ToString() + ";" + "3");
                }
                if (sch.FinalExams[i + 3].Secretary != sch.FinalExams[i + 4].Secretary)
                {
                    vl.AddViolation("secretaryChange", (i+3).ToString() + ";" + "4");
                }
            }
            return vl;
        }

        /*
        public ViolationList GetPresidentWorkloadWorstViolations(Schedule schedule)
        {
            double score = 0;
            int[] presidentWorkloads = new int[ctx.Presidents.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }

            double optimalWorkload = 100 / ctx.Presidents.Length;

            foreach (Instructor pres in ctx.Presidents)
            {
                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.5)
                {
                    score += TS_Scores.PresidentWorkloadWorst;
                }

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.5)
                {
                    score += TS_Scores.PresidentWorkloadWorst;
                }

            }

            return score;
        }
       
        public ViolationList GetPresidentWorkloadWorseViolations(Schedule schedule)
        {
            double score = 0;
            int[] presidentWorkloads = new int[ctx.Presidents.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }


            double optimalWorkload = 100 / ctx.Presidents.Length;

            foreach (Instructor pres in ctx.Presidents)
            {

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.7 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] >= optimalWorkload * 0.5)
                {
                    score += TS_Scores.PresidentWorkloadWorse;
                }

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.3 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] <= optimalWorkload * 1.5)
                {
                    score += TS_Scores.PresidentWorkloadWorse;
                }

            }

            return score;
        }

        public ViolationList GetPresidentWorkloadBadViolations(Schedule schedule)
        {
            double score = 0;
            int[] presidentWorkloads = new int[ctx.Presidents.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }


            double optimalWorkload = 100 / ctx.Presidents.Length;

            foreach (Instructor pres in ctx.Presidents)
            {

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.9 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] >= optimalWorkload * 0.7)
                {
                    score += TS_Scores.PresidentWorkloadBad;
                }

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.1 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] <= optimalWorkload * 1.3)
                {
                    score += TS_Scores.PresidentWorkloadBad;
                }
            }

            return score;
        }

        public ViolationList GetSecretaryWorkloadWorstViolations(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }

            double optimalWorkload = 100 / ctx.Secretaries.Length;

            foreach (Instructor secr in ctx.Secretaries)
            {
                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 0.5)
                {
                    score += TS_Scores.SecretaryWorkloadWorst;
                }

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 1.5)
                {
                    score += TS_Scores.SecretaryWorkloadWorst;
                }

            }

            return score;
        }

        public double GetSecretaryWorkloadWorseScore(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }


            double optimalWorkload = 100 / ctx.Secretaries.Length;

            foreach (Instructor secr in ctx.Secretaries)
            {

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 0.7 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] >= optimalWorkload * 0.5)
                {
                    score += TS_Scores.SecretaryWorkloadWorse;
                }

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 1.3 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] <= optimalWorkload * 1.5)
                {
                    score += TS_Scores.SecretaryWorkloadWorse;
                }

            }

            return score;
        }

        public double GetSecretaryWorkloadBadScore(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }


            double optimalWorkload = 100 / ctx.Secretaries.Length;

            foreach (Instructor secr in ctx.Secretaries)
            {

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 0.9 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] >= optimalWorkload * 0.7)
                {
                    score += TS_Scores.SecretaryWorkloadBad;
                }

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 1.1 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] <= optimalWorkload * 1.3)
                {
                    score += TS_Scores.SecretaryWorkloadBad;
                }
            }

            return score;
        }

        public double GetMemberWorkloadWorstScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }

            double optimalWorkload = 100 / ctx.Members.Length;

            foreach (Instructor memb in ctx.Members)
            {
                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 0.5)
                {
                    score += TS_Scores.MemberWorkloadWorst;
                }

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 1.5)
                {
                    score += TS_Scores.MemberWorkloadWorst;
                }

            }

            return score;
        }

        public double GetMemberWorkloadWorseScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }


            double optimalWorkload = 100 / ctx.Members.Length;

            foreach (Instructor memb in ctx.Members)
            {

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 0.7 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] >= optimalWorkload * 0.5)
                {
                    score += TS_Scores.MemberWorkloadWorse;
                }

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 1.3 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] <= optimalWorkload * 1.5)
                {
                    score += TS_Scores.MemberWorkloadWorse;
                }

            }

            return score;
        }

        public double GetMemberWorkloadBadScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }

            double optimalWorkload = 100 / ctx.Members.Length;

            foreach (Instructor memb in ctx.Members)
            {

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 0.9 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] >= optimalWorkload * 0.7)
                {
                    score += TS_Scores.MemberWorkloadBad;
                }

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 1.1 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] <= optimalWorkload * 1.3)
                {
                    score += TS_Scores.MemberWorkloadBad;
                }
            }
            return score;
        }
        */

        /*
        public ViolationList GetPresidentSelfStudentScore(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            foreach (var fi in sch.FinalExams)
            {
                if ((fi.Supervisor.Roles & Roles.President) == Roles.President && fi.Supervisor != fi.President)
                {
                    score += TS_Scores.PresidentSelfStudent;
                    if (ctx.FillDetails)
                    {
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorComment += $"Not President: {TS_Scores.PresidentSelfStudent}\n";
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorScore += TS_Scores.PresidentSelfStudent;
                    }
                }
            }
            return score;
        }

        public double GetSecretarySelfStudentScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if ((fi.Supervisor.Roles & Roles.Secretary) == Roles.Secretary && fi.Supervisor != fi.Secretary)
                {
                    score += TS_Scores.SecretarySelfStudent;
                    if (ctx.FillDetails)
                    {
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorComment += $"Not Secretary: {TS_Scores.SecretarySelfStudent}\n";
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorScore += TS_Scores.SecretarySelfStudent;
                    }
                }
            }
            return score;
        }

        public double GetExaminerNotPresidentScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if ((fi.Examiner.Roles & Roles.President) == Roles.President && fi.Examiner != fi.President)
                {
                    score += TS_Scores.ExaminerNotPresident;
                    if (ctx.FillDetails)
                    {
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerComment += $"Not President: {TS_Scores.ExaminerNotPresident}\n";
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerScore += TS_Scores.ExaminerNotPresident;
                    }
                }
            }
            return score;
        }
        */
    }
}

