using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUOnlineManager.Optimizer
{
    public class MUThread
    {
        public bool InUse { get; }

        public MUThread(bool inUse)
        {
            InUse = inUse;
        }
    }
}
