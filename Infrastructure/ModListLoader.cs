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
            public List<ModDetails> m_AllMods;
        }
        public static ModsContainer instance;
        public static List<ModDetails> GetModsManifests()
        {
            using var wc = new WebClient();
            {
                string rawmanifest = wc.DownloadString("https://raw.githubusercontent.com/BarleyFlour/ModFinder_WOTR/master/ModManifest/Master_Manifest.json");

                var modsContainer = Newtonsoft.Json.JsonConvert.DeserializeObject<ModsContainer>(rawmanifest);
                
                /*foreach(var mod in modsContainer.m_AllMods)
                {
                    /*if(Main.Settings.InstalledMods.Contains(mod))
                    {
                        mod.CanInstall = false;
                    }
                    else
                    {

                    }*//*
                    if (mod.Source == ModSource.GitHub)
                    {
                        var release = Infrastructure.Main.Client.Repository.Release.GetLatest(mod.OwnerAndRepo[0], mod.OwnerAndRepo[1]);
                        mod.LatestVersion = release.Result.TagName;
                        mod.Description = (Infrastructure.Main.Client.Repository.Get(mod.OwnerAndRepo[0], mod.OwnerAndRepo[1])).Result.Description;
                    }
                    
                }*/
                instance = modsContainer;
                return instance.m_AllMods;
            }
            var result = new List<ModDetails>();
            return result;
        }
    }
}
