using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public abstract class Movement
    {
        public string ID;

        public Location FromLocation;

        public Location ToLocation;

        public int FromTime;

        public int ToTime;

        public List<ResourceSelectionGroup> ResourceSelectionGroupSet = new List<ResourceSelectionGroup>();

        public bool IsActivated = false;

        public virtual string Description
        {
            get;
        }

        public virtual void GenerateResourceSelectionGroup()
        {

        }

        public override string ToString()
        {
            return string.Format("m_{0}", ID);
        }
    }
}
