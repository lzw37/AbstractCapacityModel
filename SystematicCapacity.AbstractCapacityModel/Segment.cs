using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    public class Segment
    {
        public string ID;

        public Station FromStation;

        public Station ToStation;

        public BlockSection BindingBlockingSection;

        public int RunningTime;
    }
}
