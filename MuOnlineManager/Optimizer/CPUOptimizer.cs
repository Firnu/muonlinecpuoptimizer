using GalaSoft.MvvmLight;
using MUOnlineManager.enums;
using MUOnlineManager.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MUOnlineManager.Optimizer
{
    public class CPUOptimizer : ViewModelBase
    {
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        private const int SW_RESTORE = 9;

        private Timer refreshTimer;
        private Logger Logger;
        private bool isInputEnabled = false;

        private MUInstance selectedMUProcess;

        public MUInstance SelectedMUProcess
        {
            get
            {
                return selectedMUProcess;
            }
            set
            {
                selectedMUProcess = value;
                RaisePropertyChanged(() => SelectedMUProcess);
            }
        }

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
                RaisePropertyChanged(() => IsInputEnabled);
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
                if (process.ProcessName.ToUpper() == "MAIN" || process.ProcessName.ToUpper() == "MAIN.EXE")
                {
                    MuProcessesList.Add(new MUInstance(process, i, Logger));
                    i++;
                }
            }

            if (MuProcessesList.Count == 0)
            {
                Logger.Log("Couldn't find running MU clients. Launch any MU client and refresh the list.");
                IsInputEnabled = false;
            }
            else
            {
                Logger.Log("Found " + MuProcessesList.Count() + " running MU instances.");
                IsInputEnabled = true;
            }
        }

        public void BringToFront()
        {
            if (SelectedMUProcess == null)
            {
                Logger.Log("No MU process selected.");
                return;
            }

            IntPtr handle = SelectedMUProcess.MainWindowHandle;
            if (IsIconic(handle))
            {
                ShowWindow(handle, SW_RESTORE);
            }

            SetForegroundWindow(handle);
        }

        public void ChangeName()
        {
            if (SelectedMUProcess == null)
            {
                Logger.Log("No MU process selected.");
                return;
            }
        }

        public void SetPriority(ProcessPriorityClass mode)
        {
            foreach (MUInstance instance in MuProcessesList)
            {
                instance.Priority = mode;
            }
        }

        public void SetAffinity(AffinityMode mode)
        {
            int runningClientsCount = MuProcessesList.Count;
            bool[] threads = new bool[Environment.ProcessorCount];

            if (Environment.ProcessorCount == 1)
            {
                Logger.Log("System has a single thread CPU available. Cannot work with CPU thread affinity.");
                return;
            }

            if (Environment.ProcessorCount < 4 && mode == AffinityMode.Quarter)
            {
                Logger.Log("System has less than four CPU threads available. Cannot reduce affinity usage to a quarter.");
            }

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                if (mode == AffinityMode.Full)
                {
                    threads[i] = true;
                }
                else if (mode == AffinityMode.Half)
                {
                    if (i % 2 == 0)
                    {
                        threads[i] = true;
                    }
                    else
                    {
                        threads[i] = false;
                    }
                }
                else if (mode == AffinityMode.Quarter)
                {
                    if (i % 4 == 0)
                    {
                        threads[i] = true;
                    }
                    else
                    {
                        threads[i] = false;
                    }
                }
            }

            BitArray bitArray = new BitArray(threads);

            string bits = "";
            foreach (bool bit in bitArray)
            {
                bits += Convert.ToInt32(bit);
            }

            int[] finalIntArray = new int[1];
            bitArray.CopyTo(finalIntArray, 0);
            int finalNumber = finalIntArray[0];

            foreach (MUInstance instance in MuProcessesList)
            {
                instance.Affinity = new IntPtr(finalNumber);
                instance.RefreshAffinities();
            }
        }

        private void RefreshThreadItems(object state)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(
              DispatcherPriority.Normal,
              new Action(() =>
              {
                  foreach (MUInstance instance in MuProcessesList)
                  {
                      instance.RefreshCpuUsage();
                  }
              }));
            }
        }
    }
}