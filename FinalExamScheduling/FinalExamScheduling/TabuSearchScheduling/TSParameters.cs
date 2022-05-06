using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TSParameters
    {
        //Mode of neighbour generation: Random/Greedy/Tandem
        public const string Mode = "Tandem";

        public class Random
        {
            public const int TabuLifeIterations = 20; //15

            public const int TabuListLength = 30; //40
        }

        public class Greedy
        {
            public const int TabuLifeIterations = 20; //10

            public const int TabuListLength = 1; //3
        }

        public const int GeneratedCandidates = 200; //200

        public const int AllowedIdleIterations = 15; //10

        public const double TargetCost = 0;

        public const bool GetInfo = true;

        public static int MaxFailedNeighbourGenerations = 3; //3

        public static int TandemIdleSwitches = 5; //1
    }
}
