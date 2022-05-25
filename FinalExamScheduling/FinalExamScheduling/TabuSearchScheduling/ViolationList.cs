using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            Violations.Add(new KeyValuePair<string, string>(param, value));
        }

        public void AddViolation(KeyValuePair<string, string> v)
        {
            Violations.Add(v);
        }

        public void printViolations()
        {
            if (this.Violations == null) return;

            foreach (KeyValuePair<string, string> v in this.Violations)
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
                GetSecretaryChangeViolations,

                GetPresidentWorkloadViolations,
                GetSecretaryWorkloadViolations,
                GetMemberWorkloadViolations,

                GetSupervisorNotPresidentViolations,
                GetSupervisorNotSecretaryViolations,
                GetExaminerNotPresidentViolations/*,
                GetExaminerNotSecretaryViolations,
                GetExaminerNotMemberViolations,
                GetSupervisorNotMemberViolations,
                GetSupervisorNotExaminerViolations*/

           };

            var tasks = ViolationFunctions.Select(cf => Task.Run(() => cf(sch))).ToArray();
            Task.WaitAll(tasks);
            foreach (var task in tasks)
            {
                ViolationList viRes = task.Result;
                foreach (KeyValuePair<string, string> v in viRes.Violations)
                {
                    vi.AddViolation(v);
                }
            }

            return vi;
        }

        public ViolationList GetWrongExaminerViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveWrongExaminer) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (!sch.FinalExams[i].Student.ExamCourse.Instructors.ToArray().Contains(sch.FinalExams[i].Examiner)) vl.AddViolation("wrongExaminer", i.ToString());
            }
            return vl;
        }

        public ViolationList GetStudentDuplicatedViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveStudentDuplicated) return vl;
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
            if (!TSParameters.SolvePresidentAvailability) return vl;
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
            if (!TSParameters.SolveSecretaryAvailability) return vl;
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
            if (!TSParameters.SolveExaminerAvailability) return vl;
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
            if (!TSParameters.SolveMemberAvailability) return vl;
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
            if (!TSParameters.SolveSupervisorAvailability) return vl;
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
            if (!TSParameters.SolveSecretaryChange) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i += 5)
            {
                if (sch.FinalExams[i].Secretary != sch.FinalExams[i + 1].Secretary)
                {
                    vl.AddViolation("secretaryChange", i.ToString() + ";" + "1");
                }
                if (sch.FinalExams[i + 1].Secretary != sch.FinalExams[i + 2].Secretary)
                {
                    vl.AddViolation("secretaryChange", (i + 1).ToString() + ";" + "2");
                }
                if (sch.FinalExams[i + 2].Secretary != sch.FinalExams[i + 3].Secretary)
                {
                    vl.AddViolation("secretaryChange", (i + 2).ToString() + ";" + "3");
                }
                if (sch.FinalExams[i + 3].Secretary != sch.FinalExams[i + 4].Secretary)
                {
                    vl.AddViolation("secretaryChange", (i + 3).ToString() + ";" + "4");
                }
            }
            return vl;
        }


        public ViolationList GetPresidentWorkloadViolations(Schedule schedule)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolvePresidentWorkload) return vl;
            List<KeyValuePair<string, int>> workloads = new List<KeyValuePair<string, int>>();

            foreach (FinalExam fe in schedule.FinalExams)
            {
                bool found = false;
                int oldValue = 0;
                KeyValuePair<string, int> wlToBeRemoved = new KeyValuePair<string, int>();
                foreach (KeyValuePair<string, int> wl in workloads)
                {
                    if (wl.Key.Equals(fe.President.Name))
                    {
                        found = true;
                        oldValue = wl.Value;
                        wlToBeRemoved = wl;
                    }
                }
                if (!found)
                {
                    workloads.Add(new KeyValuePair<string, int>(fe.President.Name, 1));
                }
                else
                {
                    workloads.Remove(wlToBeRemoved);

                    workloads.Add(new KeyValuePair<string, int>(fe.President.Name, (oldValue + 1)));
                }
            }
            if (workloads.Count > 1)
            {
                KeyValuePair<string, int> max = workloads.ElementAt(0);
                KeyValuePair<string, int> min = max;

                for (int i = 1; i < workloads.Count; i++)
                {
                    KeyValuePair<string, int> wl = workloads.ElementAt(i);
                    if (wl.Value < min.Value) min = wl;
                    if (wl.Value > max.Value) max = wl;
                }
                if (!ReferenceEquals(min, max)) vl.AddViolation("presidentWorkload", max.Key + ";" + max.Value + ";" + min.Key + ";" + min.Value);
            }

            return vl;
        }

        public ViolationList GetSecretaryWorkloadViolations(Schedule schedule)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveSecretaryWorkload) return vl;
            List<KeyValuePair<string, int>> workloads = new List<KeyValuePair<string, int>>();

            foreach (FinalExam fe in schedule.FinalExams)
            {
                bool found = false;
                int oldValue = 0;
                KeyValuePair<string, int> wlToBeRemoved = new KeyValuePair<string, int>();
                foreach (KeyValuePair<string, int> wl in workloads)
                {
                    if (wl.Key.Equals(fe.Secretary.Name))
                    {
                        found = true;
                        oldValue = wl.Value;
                        wlToBeRemoved = wl;
                    }
                }
                if (!found)
                {
                    workloads.Add(new KeyValuePair<string, int>(fe.Secretary.Name, 1));
                }
                else
                {
                    workloads.Remove(wlToBeRemoved);

                    workloads.Add(new KeyValuePair<string, int>(fe.Secretary.Name, (oldValue + 1)));
                }
            }
            if (workloads.Count > 1)
            {
                KeyValuePair<string, int> max = workloads.ElementAt(0);
                KeyValuePair<string, int> min = max;

                for (int i = 1; i < workloads.Count; i++)
                {
                    KeyValuePair<string, int> wl = workloads.ElementAt(i);
                    if (wl.Value < min.Value) min = wl;
                    if (wl.Value > max.Value) max = wl;
                }
                if (!ReferenceEquals(min, max)) vl.AddViolation("secretaryWorkload", max.Key + ";" + max.Value + ";" + min.Key + ";" + min.Value);
            }

            return vl;
        }
        public ViolationList GetMemberWorkloadViolations(Schedule schedule)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveMemberWorkload) return vl;
            List<KeyValuePair<string, int>> workloads = new List<KeyValuePair<string, int>>();

            foreach (FinalExam fe in schedule.FinalExams)
            {
                bool found = false;
                int oldValue = 0;
                KeyValuePair<string, int> wlToBeRemoved = new KeyValuePair<string, int>();
                foreach (KeyValuePair<string, int> wl in workloads)
                {
                    if (wl.Key.Equals(fe.Member.Name))
                    {
                        found = true;
                        oldValue = wl.Value;
                        wlToBeRemoved = wl;
                    }
                }
                if (!found)
                {
                    workloads.Add(new KeyValuePair<string, int>(fe.Member.Name, 1));
                }
                else
                {
                    workloads.Remove(wlToBeRemoved);

                    workloads.Add(new KeyValuePair<string, int>(fe.Member.Name, (oldValue + 1)));
                }
            }
            if (workloads.Count > 1)
            {
                KeyValuePair<string, int> max = workloads.ElementAt(0);
                KeyValuePair<string, int> min = max;

                for (int i = 1; i < workloads.Count; i++)
                {
                    KeyValuePair<string, int> wl = workloads.ElementAt(i);
                    if (wl.Value < min.Value) min = wl;
                    if (wl.Value > max.Value) max = wl;
                }
                if (!ReferenceEquals(min, max)) vl.AddViolation("memberWorkload", max.Key + ";" + max.Value + ";" + min.Key + ";" + min.Value);
            }

            return vl;
        }



        public ViolationList GetSupervisorNotPresidentViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveSupervisorNotPresident) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if ((fi.Supervisor.Roles & Roles.President) == Roles.President && fi.Supervisor != fi.President)
                {
                    vl.AddViolation("supervisorNotPresident", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetSupervisorNotSecretaryViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveSupervisorNotSecretary) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if ((fi.Supervisor.Roles & Roles.Secretary) == Roles.Secretary && fi.Supervisor != fi.Secretary)
                {
                    vl.AddViolation("supervisorNotSecretary", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetExaminerNotPresidentViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveExaminerNotPresident) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if ((fi.Examiner.Roles & Roles.President) == Roles.President && fi.Examiner != fi.President)
                {
                    vl.AddViolation("examinerNotPresident", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetExaminerNotSecretaryViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if ((fi.Examiner.Roles & Roles.Secretary) == Roles.Secretary && fi.Examiner != fi.Secretary)
                {
                    vl.AddViolation("examinerNotSecretary", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetExaminerNotMemberViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if ((fi.Examiner.Roles & Roles.Member) == Roles.Member && fi.Examiner != fi.Member)
                {
                    vl.AddViolation("examinerNotMember", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetSupervisorNotMemberViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if ((fi.Supervisor.Roles & Roles.Member) == Roles.Member && fi.Supervisor != fi.Member)
                {
                    vl.AddViolation("supervisorNotMember", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetSupervisorNotExaminerViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if (fi.Student.ExamCourse.Instructors.Contains(fi.Supervisor) && fi.Supervisor != fi.Examiner)
                {
                    vl.AddViolation("supervisorNotExaminer", i.ToString());
                }
            }
            return vl;
        }

    }
}

