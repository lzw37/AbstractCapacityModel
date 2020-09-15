using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public abstract class Resource
    {
        public string ID;

        public string Description;

        public ResourceStatus[] StatusArray = new ResourceStatus[Parameters.TimeHorizon];

        public List<ResourceStatus> PossibleResourceStatus = new List<ResourceStatus>();

        public List<ResourceStatusTransition> PossibleResourceStatusTransition = new List<ResourceStatusTransition>();

        public ResourceStatusTransitionRule NonOccupiedStatusTransitionRule;

        public ResourceStatus[] ResultResourceStatusArray = new ResourceStatus[Parameters.TimeHorizon + 1];

        public virtual List<ResourceStatus> GetPossibleResourceStatusByTime(int t)
        {
            return timeDependentPossibleResourceStatus[t];
        }

        private List<ResourceStatus>[] timeDependentPossibleResourceStatus = new List<ResourceStatus>[Parameters.TimeHorizon + 1];

        public void GenerateTimeDependentPossibleResourceStatus()
        {
            for (int t = 0; t <= Parameters.TimeHorizon; t++)
            {
                List<ResourceStatus> resultList = new List<ResourceStatus>();

                foreach (ResourceStatus status in PossibleResourceStatus)
                {
                    Movement m = status.HandlingMovement;

                    if (m == null)
                    {
                        resultList.Add(status);
                        continue;
                    }

                    foreach (ResourceSelectionGroup g in m.ResourceSelectionGroupSet)
                    {
                        if (!g.ResourceStatusRuleList.ContainsKey(this))
                            continue;
                        if (!g.ResourceStatusRuleList[this].BindingStatusDict.ContainsKey(t))
                            continue;
                        if (g.ResourceStatusRuleList[this].BindingStatusDict[t].Contains(status))
                        {
                            resultList.Add(status);
                        }
                    }
                }
                timeDependentPossibleResourceStatus[t] = resultList;
            }
        }

        public virtual void GeneratePossibleResourceStatus()
        {

        }

        public override string ToString()
        {
            return this.ID;
        }
    }

    public abstract class ResourceStatus
    {
        public Movement HandlingMovement;
    }

    public abstract class ResourceStatusTransition
    {
        public ResourceStatus OrgStatus;

        public ResourceStatus FinalStatus;

        public override string ToString()
        {
            return string.Format("{0}__{1}", OrgStatus.ToString(), FinalStatus.ToString());
        }
    }
}
