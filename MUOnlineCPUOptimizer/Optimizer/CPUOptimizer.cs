using MUOnlineCPUOptimizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUOnlineCPUOptimizer.Optimizer
{
    public class CPUOptimizer
    {
        private Logger Logger;
        public ObservableCollection<MUInstance> MuProcessesList { get; }

        public CPUOptimizer(Logger logger)
        {
            Logger = logger;
            MuProcessesList = new ObservableCollection<MUInstance>();
        }

        public void GetMUProcesses()
        {
            Logger.Log("Getting list of running MU mains.");
            MuProcessesList.Clear();

            Process[] proc = Process.GetProcesses();

            foreach (Process process in proc)
            {
                if(process.ProcessName == "main")
                {
                    Logger.Log("Found running MU instance.");
                    MuProcessesList.Add(new MUInstance(process));
                }
            }
        }
    }
}
