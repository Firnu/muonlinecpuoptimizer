using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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

        public MainWindowViewModel()
        {
            Logger = new Logger();
            Logger.Log("MU Online CPU Optimizer started.");

            RefreshMuProcessess = new RelayCommand(() => CpuOptimizer.GetMUProcesses());

            CpuOptimizer = new CPUOptimizer(Logger);
            CpuOptimizer.GetMUProcesses();
        }
    }
}
