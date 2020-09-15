using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public class VehicleWaitingMovement : Movement
    {
        public Vehicle BindingVehicle;

        public override string Description
        {
            get
            {
                return BindingVehicle.ToString();
            }
        }

        public override void GenerateResourceSelectionGroup()
        {
            // Only vehicle resource
            ResourceSelectionGroup group = new ResourceSelectionGroup()
            {
                ID = this.ResourceSelectionGroupSet.Count.ToString(),
                ResourceType = typeof(Vehicle),
                ResourceList = new List<Resource>() { BindingVehicle },
            };

            ResourceSelectionGroupSet.Add(group);

            ResourceStatusRule rule = new ResourceStatusRule();
            group.ResourceStatusRuleList.Add(BindingVehicle, rule);

            // status
            var toTimePossibleStatus = BindingVehicle.PossibleResourceStatus.Find(x => ((VehicleStatus)x).HandlingMovement == this);
            rule.BindingStatusDict.Add(ToTime, new List<ResourceStatus>() { toTimePossibleStatus });

            if (FromTime != 0)
            {
                // previous status
                var fromTimePossibleStatus = BindingVehicle.PossibleResourceStatus.FindAll(
                    x => ((VehicleStatus)x).Location == FromLocation &&
                 ((VehicleStatus)x).HandlingMovement.ToTime == FromTime - 1);
                rule.NecessityStatusDict.Add(FromTime - 1, fromTimePossibleStatus);
            }

            if (ToTime != Parameters.TimeHorizon)
            {
                // next moment status
                var nextMomentPossibleStatus = BindingVehicle.PossibleResourceStatus.FindAll(x => ((VehicleStatus)x).Location == FromLocation &&
                   ((VehicleStatus)x).HandlingMovement.FromTime == ToTime + 1);
                rule.NecessityStatusDict.Add(ToTime + 1, nextMomentPossibleStatus);
            }
        }
    }
}
