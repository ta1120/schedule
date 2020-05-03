﻿using FinalExamScheduling.Model;
using Gurobi;

namespace FinalExamScheduling.LPScheduling
{
    ///Az osztály feladata a modell életciklusának kezelése
    ///ez egy singleton osztály
    public class GurobiController
    {
        private GRBEnv env;
        private GRBModel model;
        public GRBModel Model { get { return model; } }

        private static GurobiController instance = null;
        private GurobiController()
        {
            env = new GRBEnv(true);
            env.Set("LogFile", "mip1.log");
            env.Start();
            model = new GRBModel(env);
        }
        public static GurobiController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GurobiController();
                }
                return instance;
            }
        }
        public void Dispose()
        {
            model.Dispose();
            env.Dispose();
        }

        public void DualDim(GRBVar[,] varp, Entity[] entp, GRBVar[,] varq, Entity[] entq, int dim)
        {
            for (int ts = 0; ts < dim; ts++)
            {
                for (int i = 0; i < entp.Length; i++)
                {
                    varp[i, ts] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, entp[i].Name + " " + ts);
                }
                for (int s = 0; s < entq.Length; s++)
                {
                    varq[s, ts] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, entq[s].Name + " " + ts);
                }
            }
        }

        public void SingleDim(GRBVar[] varp, GRBVar[] varq, Entity[] ent)
        {
            for (int i = 0; i < ent.Length; i++)
            {
                varp[i] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, ent[i].Name + "_workload_P");
                varq[i] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, ent[i].Name + "_workload_Q");
            }
        }
    }
}
