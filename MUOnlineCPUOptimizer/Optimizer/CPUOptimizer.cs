using GalaSoft.MvvmLight;
using MUOnlineCPUOptimizer.enums;
using MUOnlineCPUOptimizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MUOnlineCPUOptimizer.Optimizer
{
    public class CPUOptimizer : ViewModelBase
    {
        private Timer refreshTimer; 
        private Logger Logger;
        private IntPtr maxAffinity;
        private IntPtr halfAffinity;
        private IntPtr quarterAffinity;
        private bool isInputEnabled = false;

        public ObservableCollection<MUInstance> MuProcessesList { get; }

        public bool IsInputEnabled
        {
            get
            {
                return isInputEnabled;
            }
            set
            {
                isInputEnabled = value;
                RaisePropertyChanged(()=> IsInputEnabled);
            }
        }

        public CPUOptimizer(Logger logger)
        {
            Logger = logger;
            Logger.Log("Maximum number of threads for the system is: " + Environment.ProcessorCount);

            MuProcessesList = new ObservableCollection<MUInstance>();
            refreshTimer = new Timer(RefreshThreadItems, null, 100, 1000);
            GetMUProcesses();
        }

        public void GetMUProcesses()
        {
            Logger.Log("Getting list of running MU mains.");
            MuProcessesList.Clear();

            Process[] proc = Process.GetProcesses();

            int i = 0;
            foreach (Process process in proc)
            {
                if(process.ProcessName == "GalaxyClient")
                {
                    Logger.Log("Found running MU instance.");
                    MuProcessesList.Add(new MUInstance(process, i, Logger));
                    i++;
                }
            }

            if(MuProcessesList.Count == 0)
            {
                Logger.Log("Couldn't find running MU clients. Launch any MU client and refresh the list.");
                IsInputEnabled = false;
            }
            else
            {
                IsInputEnabled = true;
            }
        }

        public void SetAffinity(AffinityMode mode)
        {
            int runningClientsCount = MuProcessesList.Count;

            IntPtr newAffinity;
            switch (mode)
            {
                case AffinityMode.Full:
                    Logger.Log("Setting all MU clients to use all available CPU threads (affinity).");
                    newAffinity = new IntPtr((int)(Math.Pow(Environment.ProcessorCount, 2) - 1));
                    break;
                case AffinityMode.Half:
                    Logger.Log("Setting all MU clients to use half available CPU threads (affinity).");
                    newAffinity = new IntPtr((int)(Math.Pow(Environment.ProcessorCount, 2) - 1));
                    newAffinity = halfAffinity;
                    break;
                case AffinityMode.Quarter:
                    Logger.Log("Setting all MU clients to use a quarter of available CPU threads (affinity).");
                    newAffinity = new IntPtr((int)(Math.Pow(Environment.ProcessorCount, 2) - 1));
                    newAffinity = quarterAffinity;
                    break;
                case AffinityMode.Spread:
                    Logger.Log("Spreading all MU clients evenly among available CPU threads (affinity) - one thread per client if possible.");
                    newAffinity = new IntPtr((int)(Math.Pow(Environment.ProcessorCount, 2) - 1));
                    newAffinity = quarterAffinity;
                    break;
                default:
                    Logger.Log("No affinity mode specified. Defaulting to use all available CPU affinity.");
                    newAffinity = new IntPtr((int)(Math.Pow(Environment.ProcessorCount, 2) - 1));
                    break;
            }

            foreach (MUInstance instance in MuProcessesList)
            {
                instance.Affinity = newAffinity;
            }
        }

        private void RefreshThreadItems(object state)
        {
            if(Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(
              DispatcherPriority.Normal,
              new Action(() => {
                  foreach (MUInstance instance in MuProcessesList)
                  {
                      instance.RefreshCpuUsage();
                  }
              }));
            }   
        }
    }
}
