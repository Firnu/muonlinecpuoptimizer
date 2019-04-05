using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUOnlineManager.Settings
{
    public static class SettingsManager
    {
        private readonly static string settingsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "/Settings");
        private readonly static string fileName = "Settings.json";
        private static string FullPath { get => Path.Combine(settingsFolder, fileName); }

        public static GlobalSettings Settings { get; set; }

        private static void LoadDefault()
        {
            Settings = GlobalSettings.GetDefault();
            SaveSettings();
        }

        public static void SaveSettings()
        {
            Directory.CreateDirectory(settingsFolder);
            string output = JsonConvert.SerializeObject(Settings);
            File.WriteAllText(FullPath, output);
        }

        public static void LoadSettings()
        {
            if (File.Exists(FullPath))
            {
                string input = File.ReadAllText(FullPath);
                Settings = JsonConvert.DeserializeObject<GlobalSettings>(input);
            }
            else
            {
                LoadDefault();
            } 
        }
    }
}
