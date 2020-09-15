using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public abstract class Location
    {
        public string ID;

        public override string ToString()
        {
            return ID;
        }
    }
}
