using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public class CapacityEstimationModel
    {
        GRBEnv environment;
        GRBModel model;

        public void Preprocess()
        {
            // generate movement
            DateTime startTime = DateTime.Now;
            // normal movement
            foreach (Train tr in DataRepository.TrainList)
            {
                tr.GenerateCandidateMovement();
            }

            // vehicle waiting movement
            foreach (Resource r in DataRepository.ResourceList)
            {
                if (r is Vehicle)
                {
                    foreach (Location sta in DataRepository.LocationList)
                    {
                        if (!(sta is Station))
                            continue;
                        for (int t = 0; t <= Parameters.TimeHorizon; t++)
                        {
                            VehicleWaitingMovement m = new VehicleWaitingMovement()
                            {
                                ID = DataRepository.MovementList.Count.ToString(),
                                BindingVehicle = (Vehicle)r,
                                FromLocation = sta,
                                ToLocation = sta,
                                FromTime = t,
                                ToTime = t,
                            };
                            DataRepository.MovementList.Add(m);
                        }
                    }
                }
            }

            // possible resource status
            Parallel.ForEach<Resource>(DataRepository.ResourceList, r =>
            //foreach (Resource r in DataRepository.ResourceList)
            {
                r.GeneratePossibleResourceStatus();
            }
            );

            // resource selection for movements
            Parallel.ForEach<Movement>(DataRepository.MovementList, m =>
            //foreach (Movement m in DataRepository.MovementList)
            {
                m.GenerateResourceSelectionGroup();
            }
            );

            // time dependent resource possibility for resources
            Parallel.ForEach<Resource>(DataRepository.ResourceList, r =>
            //foreach (Resource r in DataRepository.ResourceList)
            {
                r.GenerateTimeDependentPossibleResourceStatus();
            }
            );

            // output debug data
            if (Parameters.DebugLog)
            {
                DataRepository.OutputMovements();
                DataRepository.OutputResourceStatus();
                DataRepository.OutputMovementResource();
            }

            Console.WriteLine("Preprocessing time: {0}", (DateTime.Now - startTime).TotalSeconds);
        }


        public void Solve()
        {
            GenerateModel();

            model.Optimize();

            if (model.Status == GRB.Status.OPTIMAL)
            {
                model.Write("solution.sol");
                InterpretSolution();
            }
        }

        private void GenerateModel()
        {
            environment = new GRBEnv("gurobi.log");
            model = new GRBModel(environment);

            GenerateVariables();
            model.Update();

            GenerateObj();
            GenerateConst();

            model.Write("model.lp");
        }

        private void GenerateVariables()
        {
            // x: Movement selection
            foreach (Movement m in DataRepository.MovementList)
            {
                GRBVar x = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "x_" + m.ToString());
            }

            // y: resource status selection
            foreach (Resource r in DataRepository.ResourceList)
            {
                for (int t = 0; t <= Parameters.TimeHorizon; t++)
                {
                    foreach (ResourceStatus status in r.GetPossibleResourceStatusByTime(t))
                    {
                        GRBVar y = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, string.Format("y_{0}_{1}_{2}", r.ToString(), t, status.ToString()));
                    }
                }
            }

            // h: movement to resource binding 
            foreach (Movement m in DataRepository.MovementList)
            {
                foreach (ResourceSelectionGroup resourceGroup in m.ResourceSelectionGroupSet)
                {
                    foreach (Resource r in resourceGroup.ResourceList)
                    {
                        GRBVar h = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, string.Format("h_{0}_{1}", m.ToString(), r.ToString()));
                    }
                }
            }
        }

        private void GenerateObj()
        {
            GRBLinExpr objExpr = 0;

            // maximizing the total amount of movements
            foreach (Movement m in DataRepository.MovementList)
            {
                if (m is TrainSegmentMovement)
                    objExpr += model.GetVarByName("x_" + m.ToString());
            }
            model.SetObjective(objExpr, GRB.MAXIMIZE);
        }

        private void GenerateConst()
        {
            // group 1: movement time-space connectivity
            GenerateMovementConnectivityConst();

            // group 2: resource occupation (by movement)
            GenerateResourceSelectionConst();
            GenerateResourceStatusConst();

            // group 3: resource rule
            GenerateResourceStatusUniquenessConst();
        }

        private void GenerateMovementConnectivityConst()
        {
            //GRBVar x = model.GetVarByName("x_m_0");
            //model.AddConstr(x == 1, "a1");
        }

        private void GenerateResourceSelectionConst()
        {
            foreach (Movement m in DataRepository.MovementList)
            {
                foreach (ResourceSelectionGroup g in m.ResourceSelectionGroupSet)
                {
                    // for an active movement m, one resource has to be selected in resource selection group g 
                    GRBVar x = model.GetVarByName("x_" + m.ToString());

                    GRBLinExpr expr = 0;
                    foreach (Resource r in g.ResourceList)
                    {
                        GRBVar h = model.GetVarByName(string.Format("h_{0}_{1}", m.ToString(), r.ToString()));
                        expr += h;
                    }
                    model.AddConstr(x == expr, string.Format("ct_res_sel_{0}_{1}", m.ToString(), g.ToString()));
                }
            }
        }

        private void GenerateResourceStatusConst()
        {
            foreach (Movement m in DataRepository.MovementList)
            {
                foreach (ResourceSelectionGroup g in m.ResourceSelectionGroupSet)
                {
                    foreach (Resource r in g.ResourceList)
                    {
                        // if movement m uses resource r, the time-dependent status of resource r is limited to a subset
                        GRBVar h = model.GetVarByName(string.Format("h_{0}_{1}", m.ToString(), r.ToString()));

                        foreach (int t in g.ResourceStatusRuleList[r].BindingStatusDict.Keys)
                        {
                            GRBLinExpr expr = 0;
                            foreach (ResourceStatus status in g.ResourceStatusRuleList[r].BindingStatusDict[t])
                            {
                                if (!r.GetPossibleResourceStatusByTime(t).Contains(status))
                                    continue;

                                GRBVar y = model.GetVarByName(string.Format("y_{0}_{1}_{2}", r.ToString(), t, status.ToString()));
                                expr += y;
                            }
                            model.AddConstr(h == expr, string.Format("ct_res_sta_{0}_{1}_{2}", m.ToString(), r.ToString(), t));
                        }

                        foreach(int t in g.ResourceStatusRuleList[r].NecessityStatusDict.Keys)
                        {
                            GRBLinExpr expr = 0;
                            foreach(ResourceStatus status in g.ResourceStatusRuleList[r].NecessityStatusDict[t])
                            {
                                if (!r.GetPossibleResourceStatusByTime(t).Contains(status))
                                    continue;

                                GRBVar y = model.GetVarByName(string.Format("y_{0}_{1}_{2}", r.ToString(), t, status.ToString()));
                                expr += y;
                            }
                            model.AddConstr(h <= expr, string.Format("ct_res_sta_{0}_{1}_{2}", m.ToString(), r.ToString(), t));
                        }
                    }
                }
            }
        }

        private void GenerateResourceStatusUniquenessConst()
        {
            foreach (Resource r in DataRepository.ResourceList)
            {
                for (int t = 0; t <= Parameters.TimeHorizon; t++)
                {
                    // at each moment t, for each resource r, one and only one status is assigned  
                    GRBLinExpr expr = 0;
                    foreach (ResourceStatus status in r.GetPossibleResourceStatusByTime(t))
                    {
                        GRBVar y = model.GetVarByName(string.Format("y_{0}_{1}_{2}", r.ToString(), t, status.ToString()));
                        expr += y;
                    }
                    model.AddConstr(expr == 1, string.Format("ct_res_uni_{0}_{1}", r.ToString(), t));
                }
            }
        }

        private void InterpretSolution()
        {
            // x: Movement selection
            foreach (Movement m in DataRepository.MovementList)
            {
                GRBVar x = model.GetVarByName("x_" + m.ToString());
                if (x.X > 0.9)
                {
                    m.IsActivated = true;
                }
                else
                {
                    m.IsActivated = false;
                }
            }

            // y: resource status selection
            foreach (Resource r in DataRepository.ResourceList)
            {
                for (int t = 0; t <= Parameters.TimeHorizon; t++)
                {
                    foreach (ResourceStatus status in r.GetPossibleResourceStatusByTime(t))
                    {
                        GRBVar y = model.GetVarByName(string.Format("y_{0}_{1}_{2}", r.ToString(), t, status.ToString()));
                        if (y.X > 0.9)
                        {
                            r.ResultResourceStatusArray[t] = status;
                        }
                    }
                }
            }

            // h: movement to resource binding 
            foreach (Movement m in DataRepository.MovementList)
            {
                foreach (ResourceSelectionGroup resourceGroup in m.ResourceSelectionGroupSet)
                {
                    foreach (Resource r in resourceGroup.ResourceList)
                    {
                        GRBVar h = model.GetVarByName(string.Format("h_{0}_{1}", m.ToString(), r.ToString()));
                        if (h.X > 0.9)
                        {
                            resourceGroup.SelectedResource = r;
                        }
                    }
                }
            }
        }
    }
}
