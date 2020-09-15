using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public static class Parameters
    {
        public static int TimeHorizon = 60;

        public static bool DebugLog = true;

        public static OnWayLocation VirtualOnWayLocation = new OnWayLocation() { ID = "-1" };
    }
}
