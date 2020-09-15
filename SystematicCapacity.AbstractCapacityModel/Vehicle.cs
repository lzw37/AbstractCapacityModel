using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public class Vehicle : Resource
    {
        public override void GeneratePossibleResourceStatus()
        {
            foreach (Movement m in DataRepository.MovementList)
            {
                if (m is VehicleWaitingMovement)
                {
                    VehicleWaitingMovement vwm = m as VehicleWaitingMovement;
                    if (vwm.BindingVehicle == this)
                    {
                        VehicleStatus status = new VehicleStatus() { Location = vwm.FromLocation, HandlingMovement = m};
                        PossibleResourceStatus.Add(status);
                    }
                }
                else if (m is TrainSegmentMovement)
                {
                    VehicleStatus status = new VehicleStatus() { Location = m.FromLocation, HandlingMovement = m };
                    PossibleResourceStatus.Add(status);

                    status = new VehicleStatus() { Location = m.ToLocation, HandlingMovement = m };
                    PossibleResourceStatus.Add(status);

                    status = new VehicleStatus() { Location = Parameters.VirtualOnWayLocation, HandlingMovement = m };
                    PossibleResourceStatus.Add(status);
                }
            }
        }

        public override List<ResourceStatus> GetPossibleResourceStatusByTime(int t)
        {
            return base.GetPossibleResourceStatusByTime(t);
        }
    }

    public class VehicleStatus : ResourceStatus
    {
        public Location Location;

        public override string ToString()
        {
            if (HandlingMovement == null)
                return string.Format("VehS_{0}_null", Location.ToString());

            return string.Format("VehS_{0}_{1}", Location.ToString(), HandlingMovement.ToString());
        }
    }

    public class VehicleStatusTransition : ResourceStatusTransition
    {

    }
}
