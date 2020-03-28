using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUOnlineManager.ViewModels
{
    public class Logger : ViewModelBase
    {
        private readonly string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "/Logs");

        private string thisSessionLog;

        public string ThisSessionLog {
            get => thisSessionLog;
            set
            {
                thisSessionLog = value;
                RaisePropertyChanged();
            } 
        }

        public void Log(string message)
        {
            Directory.CreateDirectory(baseDirectory);

            string fileName = "Log_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
            string destPath = Path.Combine(baseDirectory, fileName);
            string logTime = "[" + DateTime.Now.ToString() + "]";

            string finalMsg = logTime + " " + message + "\n";
            File.AppendAllText(destPath, finalMsg);
            ThisSessionLog += finalMsg;
        }    
    }
}
