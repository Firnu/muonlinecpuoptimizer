using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MUOnlineCPUOptimizer.Optimizer
{
    public class MUInstance
    {
        private Process MUProcess;

        public string ProcessName { get => MUProcess.ProcessName; }

        public string ProcessFileName { get => MUProcess.MainModule.FileName; }

        public int BasePriority { get => MUProcess.BasePriority; }

        public bool HasExited { get => MUProcess.HasExited; }

        public string MainWindowTitle { get => MUProcess.MainWindowTitle; }

        public IntPtr Affinity { get => MUProcess.ProcessorAffinity; }

        public ObservableCollection<MUThread> Threads { get; }

        public MUInstance(Process MUProcess)
        {
            Threads = new ObservableCollection<MUThread>();
            this.MUProcess = MUProcess;
            
            foreach(ProcessThread thread in MUProcess.Threads)
            {
                Threads.Add(new MUThread(thread));            
            }
        }
    }
}
