using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MUOnlineManager.enums;
using MUOnlineManager.Optimizer;
using MUOnlineManager.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUOnlineManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public Logger Logger { get; }
        public CPUOptimizer CpuOptimizer { get; }
        public MULauncher MULauncher { get; }

        public RelayCommand RefreshMuProcessess { get; set; }
        public RelayCommand SetFullAffinity { get; set; }
        public RelayCommand SetHalfAffinity { get; set; }
        public RelayCommand SetQuarterAffinity { get; set; }

        public RelayCommand SetSpreadAffinityOne { get; set; }
        public RelayCommand SetSpreadAffinityTwo { get; set; }
        public RelayCommand SetSpreadAffinityThree { get; set; }

        public RelayCommand SetNormalPriority { get; set; }
        public RelayCommand SetLowPriority { get; set; }

        public RelayCommand BringToFront { get; set; }
        public RelayCommand ChangeName { get; set; }

        public RelayCommand ShowAbout { get; set; }

        public MainWindowViewModel()
        {
            Logger = new Logger();
            Logger.Log("MU Online CPU Optimizer started.");

            Logger.Log("Loading settings.");
            SettingsManager.LoadSettings();

            RefreshMuProcessess = new RelayCommand(() => CpuOptimizer.GetMUProcesses());
            SetFullAffinity = new RelayCommand(() => CpuOptimizer.SetAffinity(AffinityMode.Full));
            SetHalfAffinity = new RelayCommand(() => CpuOptimizer.SetAffinity(AffinityMode.Half));
            SetQuarterAffinity = new RelayCommand(() => CpuOptimizer.SetAffinity(AffinityMode.Quarter));
            SetSpreadAffinityOne = new RelayCommand(() => CpuOptimizer.SetAffinity(AffinityMode.Spread_One));
            SetSpreadAffinityTwo = new RelayCommand(() => CpuOptimizer.SetAffinity(AffinityMode.Spread_Two));
            SetSpreadAffinityThree = new RelayCommand(() => CpuOptimizer.SetAffinity(AffinityMode.Spread_Three));
            BringToFront = new RelayCommand(() => CpuOptimizer.BringToFront());
            ChangeName = new RelayCommand(() => CpuOptimizer.ChangeName());
            ShowAbout = new RelayCommand(() => Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "About.txt")));

            SetNormalPriority = new RelayCommand(() => CpuOptimizer.SetPriority(ProcessPriorityClass.Normal));
            SetLowPriority = new RelayCommand(() => CpuOptimizer.SetPriority(ProcessPriorityClass.BelowNormal));

            CpuOptimizer = new CPUOptimizer(Logger);
            MULauncher = new MULauncher(Logger);
        }
    }
}