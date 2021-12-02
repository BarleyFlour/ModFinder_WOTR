using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModFinder_WOTR.Infrastructure
{
    public static class ModListLoader
    {

        public class ModsContainer
        {
            [JsonProperty]
            public Dictionary<string, ModInfo> m_AllMods;
        }
        public static ModsContainer instance;
        public static Dictionary<string, ModInfo> GetModsManifests()
        {
            using var wc = new WebClient();
            {
                string rawmanifest = wc.DownloadString("https://github.com/BarleyFlour/ModFinderWOTR/blob/master/ModFinderWOTR/ModManifest/Master_Manifest.json");

                var modsContainer = Newtonsoft.Json.JsonConvert.DeserializeObject<ModsContainer>(rawmanifest);
                instance = modsContainer;
            }
            var result = new Dictionary<string, ModInfo>();
            return result;
        }
    }
}
