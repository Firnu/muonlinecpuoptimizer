using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using MUOnlineManager.Settings;
using MUOnlineManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MUOnlineManager.Optimizer
{
    public class MULauncher : ViewModelBase
    {
        private Logger Logger;

        public RelayCommand SelectMainExe { get; set; }

        public RelayCommand DecreaseClientsNumber { get; set; }
        public RelayCommand IncreaseClientsNumber { get; set; }

        public RelayCommand Launch { get; set; }

        public bool IsReadyToLaunch
        {
            get
            {
                if(SettingsManager.Settings.MainFilePath == "")
                {
                    return false;
                }

                if (File.Exists(SettingsManager.Settings.MainFilePath))
                {
                    return true;
                }

                return false;
            }
        }

        public string ClientFilePath
        {
            get
            {
                return SettingsManager.Settings.MainFilePath;
            }
        }

        public int ClientsToLaunch
        {
            get
            {
                return SettingsManager.Settings.MUClientsToLaunch;
            }
            set
            {
                SettingsManager.Settings.MUClientsToLaunch = value;
                SettingsManager.SaveSettings();
                RaisePropertyChanged(() => ClientsToLaunch);
            }
        }

        public MULauncher(Logger logger)
        {
            Logger = logger;

            IncreaseClientsNumber = new RelayCommand(() => ClientsToLaunch++);
            DecreaseClientsNumber = new RelayCommand(DecreaseClientsToLaunch);
            Launch = new RelayCommand(LaunchClients);
            SelectMainExe = new RelayCommand(SelectMUExe);
        }

        private void LaunchClients()
        {
            Logger.Log("Launching " + SettingsManager.Settings.MUClientsToLaunch + " additional MU online clients.");

            Task.Run(() =>
            {
                for (int i = 0; i < SettingsManager.Settings.MUClientsToLaunch; i++)
                {
                    ProcessStartInfo proc = new ProcessStartInfo();

                    proc.UseShellExecute = true;
                    proc.WorkingDirectory = Path.GetDirectoryName(SettingsManager.Settings.MainFilePath);
                    proc.FileName = SettingsManager.Settings.MainFilePath;
                    proc.Verb = "runas";

                    try
                    {
                        Process.Start(proc);
                    }

                    catch               
                    {
                        // The user refused the elevation.
                        // Do nothing and return directly ...
                        return;
                    }

                    Thread.Sleep(500);
                }
            });     
        }

        private void SelectMUExe()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "main.exe (*.exe)|*.exe";

            if (openFileDialog.ShowDialog() == true)
            {
                SettingsManager.Settings.MainFilePath = openFileDialog.FileName;
                SettingsManager.SaveSettings();
            }

            RaisePropertyChanged(() => IsReadyToLaunch);
            RaisePropertyChanged(() => ClientFilePath);
        }

        private void DecreaseClientsToLaunch()
        {
            if(ClientsToLaunch > 1)
            {
                ClientsToLaunch--;
            }
        }
    }
}
