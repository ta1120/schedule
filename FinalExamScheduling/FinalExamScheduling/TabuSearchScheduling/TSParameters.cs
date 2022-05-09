using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TSParameters
    {
        
        //Switches
        public const bool AllowShuffleWhenStuck = true;

        public const bool MuteConsoleUnlessDone = false;

        public const bool PrintDetails = true;

        public const bool OptimizeSoftConstraints = true;

        public const bool RestartUntilTargetReached = false;

        public const bool LogIterationalProgress = true;


        //Mode of neighbour generation: Random/Greedy/Tandem
        public const string Mode = "Tandem";

        //Tabu parameters
        public class Random
        {
            public const int TabuLifeIterations = 15; //15

            public const int TabuListLength = 30; //40
        }

        public class Greedy
        {
            public const int TabuLifeIterations = 10; //10

            public const int TabuListLength = 1; //3
        }

        //Other parameters
        public const int MaxShuffles = 1;

        public const int ShufflePercentage = 20;

        public const int GeneratedCandidates = 25; //200

        public const int AllowedIdleIterations = 10; //10

        public const double TargetScore = 0;

        public const bool GetInfo = true;

        public static int MaxFailedNeighbourGenerations = 5; //3

        public static int TandemIdleSwitches = 3; //1

        //Optimalization switches for distinct constraints
        public const bool SolveWrongExaminer = true;
        public const bool SolveStudentDuplicated = true;
        public const bool SolvePresidentAvailability = true;
        public const bool SolveSecretaryAvailability = true;
        public const bool SolveExaminerAvailability = true;
        public const bool SolveSupervisorAvailability = true;
        public const bool SolveMemberAvailability = true;
        public const bool SolvePresidentChange = true;
        public const bool SolveSecretaryChange = true;

        public const bool SolvePresidentWorkload = true;
        public const bool SolveSecretaryWorkload = true;
        public const bool SolveMemberWorkload = true;
        public const bool SolveExaminerNotPresident = true;
        public const bool SolveSupervisorNotPresident = true;
        public const bool SolveSupervisorNotSecretary = true;
    }
}
