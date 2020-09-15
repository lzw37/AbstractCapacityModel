using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public class BlockSection : Resource
    {
        public override void GeneratePossibleResourceStatus()
        {
            foreach(Movement m in DataRepository.MovementList)
            {
                if (!(m is TrainSegmentMovement))
                    continue;

                if (((TrainSegmentMovement)m).OnSegment.BindingBlockingSection != this)
                    continue;

                BlockSectionStatus OccupiedBSStatus = new BlockSectionStatus() { HandlingMovement =  m};
                PossibleResourceStatus.Add(OccupiedBSStatus);
            }

            BlockSectionStatus NonOccupiedBSStatus = new BlockSectionStatus() { HandlingMovement = null };
            PossibleResourceStatus.Add(NonOccupiedBSStatus);
        }

        public override List<ResourceStatus> GetPossibleResourceStatusByTime(int t)
        {
            return base.GetPossibleResourceStatusByTime(t);
        }
    }

    public class BlockSectionStatus : ResourceStatus
    {
        public bool IsOccupied
        {
            get
            {
                if (HandlingMovement == null)
                    return false;
                else
                    return true;
            }
        }

        public override string ToString()
        {
            if (HandlingMovement == null)
                return "BSS_null";
            return "BSS_" + HandlingMovement.ToString();
        }
    }

    public class BlockSectionStatusTransition : ResourceStatusTransition
    {

    }
}
