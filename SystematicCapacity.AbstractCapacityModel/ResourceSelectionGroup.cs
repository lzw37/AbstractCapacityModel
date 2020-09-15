using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public class ResourceSelectionGroup
    {
        public string ID;

        public Type ResourceType;

        public List<Resource> ResourceList = new List<Resource>();

        public Dictionary<Resource, ResourceStatusRule> ResourceStatusRuleList = new Dictionary<Resource, ResourceStatusRule>();

        public Dictionary<Resource, List<ResourceStatusTransitionRule>> ResourceStatusTransitionRuleList;

        public Resource SelectedResource;

        public override string ToString()
        {
            return ID;
        }
    }

    public class ResourceStatusRule
    {
        //public Dictionary<int, List<ResourceStatus>> PossibleResourceStatusDict = new Dictionary<int, List<ResourceStatus>> ();

        public Dictionary<int, List<ResourceStatus>> NecessityStatusDict = new Dictionary<int, List<ResourceStatus>>();

        public Dictionary<int, List<ResourceStatus>> BindingStatusDict = new Dictionary<int, List<ResourceStatus>>();
    }

    public class ResourceStatusTransitionRule
    {
        public Dictionary<int, List<ResourceStatusTransition>> PossibleResourceStatusTransitionDict =
            new Dictionary<int, List<ResourceStatusTransition>>();

    }
}
