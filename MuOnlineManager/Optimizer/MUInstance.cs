using GalaSoft.MvvmLight;
using MUOnlineManager.enums;
using MUOnlineManager.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MUOnlineManager.Optimizer
{
    public class MUInstance : ViewModelBase
    {
        private Logger Logger;
        private int index;
        private Process MUProcess;
        private PerformanceCounter cpuCounter;
        private double cpuUsage;

        public ObservableCollection<MUThread> Affinities { get; set; }

        public bool[] AffinityBytes { get; set; }

        public IntPtr MainWindowHandle { get => MUProcess.MainWindowHandle; }
        public string ProcessFileName { get => MUProcess.MainModule.FileName; }
        public string MainWindowTitle { get => MUProcess.MainWindowTitle; }

        public ProcessPriorityClass Priority
        {
            get
            {
                return MUProcess.PriorityClass;
            }

            set
            {
                MUProcess.PriorityClass = value;
                RaisePropertyChanged(() => Priority);
                RaisePropertyChanged(() => PriorityString);
            }
        }

        public string PriorityString
        {
            get
            {
                switch (Priority)
                {
                    case ProcessPriorityClass.Normal:
                        return "Normal";

                    case ProcessPriorityClass.Idle:
                        return "Idle";

                    case ProcessPriorityClass.High:
                        return "Very High";

                    case ProcessPriorityClass.RealTime:
                        return "Real Time";

                    case ProcessPriorityClass.BelowNormal:
                        return "Low";

                    case ProcessPriorityClass.AboveNormal:
                        return "High";

                    default:
                        return "";
                }
            }
        }

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

        public string ProcessName
        {
            get
            {
                string name = MUProcess.ProcessName;

                if (index != 0)
                {
                    name += "#" + index;
                }

                return name;
            }
        }

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

            this.MUProcess = MUProcess;
            cpuCounter = new PerformanceCounter("Process", "% Processor Time", this.ProcessName, true);

            Affinities = new ObservableCollection<MUThread>();
            RefreshAffinities();
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

        public void RefreshAffinities()
        {
            Affinities.Clear();

            var affinity = Affinity.ToInt32();
            var b = new BitArray(new[] { affinity });

            var bits = new bool[b.Count];
            b.CopyTo(bits, 0);

            for (var index = 0; index < Environment.ProcessorCount; index++)
            {
                Affinities.Add(new MUThread(bits[index]));
            }
        }
    }
}