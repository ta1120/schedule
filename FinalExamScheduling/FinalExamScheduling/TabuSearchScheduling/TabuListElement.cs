using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TabuListElement
    {
        public string Attribute;
        public string Value;
        public int TabuIterationsLeft;
        public int ExamSlot;

        public TabuListElement(string attr, string value,int slot)
        {
            Attribute = attr;
            Value = value;
            ExamSlot = slot;
            switch (TSParameters.Mode)
            {
                case "Random":
                    TabuIterationsLeft = TSParameters.Random.TabuLifeIterations;
                    break;
                case "Greedy":
                    TabuIterationsLeft = TSParameters.Greedy.TabuLifeIterations;
                    break;
                default:
                    TabuIterationsLeft = TSParameters.Greedy.TabuLifeIterations;
                    break;
            }
        }
    }
}
