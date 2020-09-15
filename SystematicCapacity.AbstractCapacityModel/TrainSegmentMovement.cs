using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public class TrainSegmentMovement : Movement
    {
        public Segment OnSegment;

        public Train BindingTrain;

        public override string Description
        {
            get
            {
                return BindingTrain.ToString();
            }
        }

        public override void GenerateResourceSelectionGroup()
        {
            // indicate which / how many resources a movement need

            // blocking section resource
            BlockSection bs = OnSegment.BindingBlockingSection;

            ResourceSelectionGroup bsResourceSelectionGroup = new ResourceSelectionGroup()
            {
                ID = this.ResourceSelectionGroupSet.Count.ToString(),
                ResourceType = typeof(BlockSection),
                ResourceList = new List<Resource>() { bs },
            };
            ResourceStatusRule rule = new ResourceStatusRule();
            BlockSectionStatus status = (BlockSectionStatus)bs.PossibleResourceStatus.Find(x => ((BlockSectionStatus)x).HandlingMovement == this);

            for (int t = FromTime; t <= ToTime; t++)
            {
                rule.BindingStatusDict.Add(t, new List<ResourceStatus>() { status });
                // if the movement is activated, only one status can be choosen.
            }

            bsResourceSelectionGroup.ResourceStatusRuleList.Add(bs, rule);
            ResourceSelectionGroupSet.Add(bsResourceSelectionGroup);


            // vehicle resource
            ResourceSelectionGroup vehicleResourceSelectionGroup = new ResourceSelectionGroup()
            {
                ID = this.ResourceSelectionGroupSet.Count.ToString(),
                ResourceType = typeof(Vehicle),
            };

            foreach (Resource veh in DataRepository.ResourceList)
            {
                if (!(veh is Vehicle))
                    continue;

                vehicleResourceSelectionGroup.ResourceList.Add(veh);

                rule = new ResourceStatusRule();
                var statusSet = veh.PossibleResourceStatus.FindAll(x => ((VehicleStatus)x).HandlingMovement == this);

                var fromStatus = statusSet.Find(x => ((VehicleStatus)x).Location == FromLocation);
                var toStatus = statusSet.Find(x => ((VehicleStatus)x).Location == ToLocation);
                var onWayStatus = statusSet.Find(x => ((VehicleStatus)x).Location == Parameters.VirtualOnWayLocation);

                rule.BindingStatusDict.Add(FromTime, new List<ResourceStatus>() { fromStatus});
                rule.BindingStatusDict.Add(ToTime, new List<ResourceStatus>() {toStatus });

                for(int t=FromTime+1;t<ToTime;t++)
                {
                    rule.BindingStatusDict.Add(t, new List<ResourceStatus>() { onWayStatus });
                }

                // incoming resource status
                if (FromTime > 0)
                {
                    var incomingResourceStatus = veh.PossibleResourceStatus.FindAll(x => ((VehicleStatus)x).Location == FromLocation);
                    rule.NecessityStatusDict.Add(FromTime - 1, incomingResourceStatus);
                }

                vehicleResourceSelectionGroup.ResourceStatusRuleList.Add(veh, rule);
            }

            ResourceSelectionGroupSet.Add(vehicleResourceSelectionGroup);
        }
    }
}
