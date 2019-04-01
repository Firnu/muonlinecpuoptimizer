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

namespace MUOnlineCPUOptimizer.Optimizer
{
    public class MUInstance : ViewModelBase
    {
        private Logger Logger;
        private int index;
        private Process MUProcess;
        private PerformanceCounter cpuCounter;

        public string ProcessName
        {
            get
            {
                string name = MUProcess.ProcessName;

                if(index != 0)
                {
                    name += "#" + index;
                }

                return name;
            }
        }

        public string ProcessFileName { get => MUProcess.MainModule.FileName; }

        public int BasePriority { get => MUProcess.BasePriority; }

        public bool HasExited { get => MUProcess.HasExited; }

        public string MainWindowTitle { get => MUProcess.MainWindowTitle; }

        public IntPtr Affinity
        {
            get
            {
                return MUProcess.ProcessorAffinity;
            }
            set
            {
                MUProcess.ProcessorAffinity = value;
                RaisePropertyChanged(() => Affinity);
            }
        }

        //public ObservableCollection<MUThread> Threads { get; }

        private double cpuUsage;
        public double CpuUsage
        {
            get
            {
                return cpuUsage;
            }
            set
            {
                cpuUsage = value;
                RaisePropertyChanged(() => CpuUsage);
            }
        }

        public MUInstance(Process MUProcess, int index, Logger logger)
        {
            this.Logger = logger;
            this.index = index;
            // Threads = new ObservableCollection<MUThread>();

            this.MUProcess = MUProcess;
            cpuCounter = new PerformanceCounter("Process", "% Processor Time", this.ProcessName, true);

            //foreach(ProcessThread thread in MUProcess.Threads)
            //{
            //    Threads.Add(new MUThread(thread));            
            //}
        }

        public void RefreshCpuUsage()
        {
            try
            {
                CpuUsage = Math.Round((double)cpuCounter.NextValue() / (double)Environment.ProcessorCount, 2);
            }
            catch (Exception e)
            {
                Logger.Log("Could not read CPU usage data for process: " + ProcessName);
                Logger.Log(e.Message);
                CpuUsage = 0;
            }
        }
    }
}
