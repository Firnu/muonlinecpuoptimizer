using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUOnlineManager.Settings
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GlobalSettings
    {
        [JsonProperty]
        public string MainFilePath { get; set; }

        [JsonProperty]
        public int MUClientsToLaunch { get; set; }

        public static GlobalSettings GetDefault()
        {
            GlobalSettings newDefault = new GlobalSettings()
            {
                MainFilePath = "",
                MUClientsToLaunch = 3
            };

            return newDefault;
        }
    }
}
