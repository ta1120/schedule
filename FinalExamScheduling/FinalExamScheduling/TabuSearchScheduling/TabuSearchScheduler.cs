using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TabuSearchScheduler
    {
        private Context ctx;
        private bool shouldTerminate = false;

        public TabuSearchScheduler(Context context)
        {
            ctx = context;
        }
            
        internal void Cancel()
        {
            shouldTerminate = true;
        }

        public Task<SolutionCandidate> RunAsync()
        {
            //Algorithm choice: later to be outsourced to class TSParameters
            ITabuSearchAlgorithm tabuSearchAlgorithm = new TabuSearchWithVL(ctx);

            return Task.Run<SolutionCandidate>(
                () =>
                {
                    Console.WriteLine("TabuSearch running...");
                    
                    var best = tabuSearchAlgorithm.Start();

                    return best;

                });
           
        }
    }
}
