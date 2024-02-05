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
                if (process.ProcessName.ToUpper() == "GAME" || process.ProcessName.ToUpper() == "GAME.EXE")
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
            if (Environment.ProcessorCount == 1)
            {
                Logger.Log("System has a single thread CPU available. Cannot work with CPU thread affinity.");
                return;
            }

            if (Environment.ProcessorCount < 4 && mode == AffinityMode.Quarter)
            {
                Logger.Log("System has less than four CPU threads available. Cannot reduce affinity usage to a quarter.");
                return;
            }

            for (var index = 0; index < MuProcessesList.Count; index++)
            {
                var instance = MuProcessesList[index];
                var instanceThreads = new InstanceThreads(instance);

                bool[] threads;
                if (mode == AffinityMode.Spread_One ||
                    mode == AffinityMode.Spread_Three ||
                    mode == AffinityMode.Spread_Two)
                {
                    threads = CreateSpreadAffinity(index, mode);
                }
                else
                {
                    threads = CreateSequentialAffinity(index, mode);
                }

                instanceThreads.SetThreads(threads);
            }
        }

        private bool[] CreateSpreadAffinity(int instanceIndex, AffinityMode mode)
        {
            var threads = new bool[Environment.ProcessorCount];

            switch (mode)
            {
                case AffinityMode.Spread_One:
                    threads[instanceIndex % Environment.ProcessorCount] = true;
                    break;

                case AffinityMode.Spread_Two:
                    threads[instanceIndex % Environment.ProcessorCount] = true;
                    threads[(instanceIndex + 1) % Environment.ProcessorCount] = true;
                    break;

                case AffinityMode.Spread_Three:
                    threads[instanceIndex % Environment.ProcessorCount] = true;
                    threads[(instanceIndex + 1) % Environment.ProcessorCount] = true;
                    threads[(instanceIndex + 2) % Environment.ProcessorCount] = true;
                    break;
            }

            return threads;
        }

        private bool[] CreateSequentialAffinity(int instanceIndex, AffinityMode mode)
        {
            var threads = new bool[Environment.ProcessorCount];

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

            return threads;
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

        public class InstanceThreads
        {
            public MUInstance Instance { get; }

            public bool[] Threads { get; private set; }

            public InstanceThreads(MUInstance instance)
            {
                Instance = instance;
                Threads = new bool[Environment.ProcessorCount];
            }

            public void SetThreads(bool[] threads)
            {
                Threads = threads;
                SetThreadBits(Threads);
            }

            private void SetThreadBits(bool[] threads)
            {
                var bitArray = new BitArray(threads);

                var finalIntArray = new int[1];
                bitArray.CopyTo(finalIntArray, 0);
                var finalNumber = finalIntArray[0];

                Instance.AffinityBytes = threads;
                Instance.Affinity = new IntPtr(finalNumber);
                Instance.RefreshAffinities();
            }
        }
    }
}