using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUOnlineCPUOptimizer.Optimizer
{
    public class MUThread
    {
        private ProcessThread thread;

        public int CurrentPriority { get => thread.CurrentPriority; }
        public string ThreadState { get => thread.ThreadState.ToString(); }

        public MUThread(ProcessThread thread)
        {
            this.thread = thread;
        }
    }
}
