using FinalExamScheduling.Model;
using FinalExamScheduling.TabuSearchScheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class SolutionCandidate
    {
        public Schedule Schedule;

        public double Score;

        public ViolationList VL;

        public SolutionCandidate(Schedule sch)
        {
            Schedule = sch.Clone();
            Score = -1;
        }

        public SolutionCandidate Clone()
        {
            return new SolutionCandidate(Schedule.Clone()) { VL = VL, Score = Score};
        }

    }
}
