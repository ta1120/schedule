using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.TabuSearchScheduling
{
    interface ITabuSearchAlgorithm
    {
        SolutionCandidate Start();

    }
}
