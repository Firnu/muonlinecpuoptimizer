using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MUOnlineCPUOptimizer.enums;
using MUOnlineCPUOptimizer.Optimizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUOnlineCPUOptimizer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public Logger Logger { get; }
        public CPUOptimizer CpuOptimizer { get; }
        
        public RelayCommand RefreshMuProcessess { get; set; }
        public RelayCommand SetFullAffinity { get; set; }
        public RelayCommand SetHalfAffinity { get; set; }
        public RelayCommand SetQuarterAffinity { get; set; }
        public RelayCommand SetSpreadAffinity { get; set; }

        public MainWindowViewModel()
        {
            Logger = new Logger();
            Logger.Log("MU Online CPU Optimizer started.");

            RefreshMuProcessess = new RelayCommand(() => CpuOptimizer.GetMUProcesses());
            SetFullAffinity = new RelayCommand(() => CpuOptimizer.SetAffinity(AffinityMode.Full));
            SetHalfAffinity = new RelayCommand(() => CpuOptimizer.SetAffinity(AffinityMode.Half));
            SetQuarterAffinity = new RelayCommand(() => CpuOptimizer.SetAffinity(AffinityMode.Quarter));
            SetSpreadAffinity = new RelayCommand(() => CpuOptimizer.SetAffinity(AffinityMode.Spread));

            CpuOptimizer = new CPUOptimizer(Logger);
        }
    }
}
