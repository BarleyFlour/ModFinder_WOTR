using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ModFinder_WOTR.Infrastructure
{
    public class AppSettingsData
    {
        [JsonProperty]
        public string s_WrathPath;
        [JsonProperty]
        public string NexusAPIKey;
        [JsonProperty]
        public string GithubAPIKey;
        [JsonProperty]
        public ModDetails[] InstalledMods = new ModDetails[] { };
        public void AddInstalled(ModDetails mod)
        {
            var list = InstalledMods.ToList();
            if (InstalledMods.Any(a => a.Name == mod.Name))
            {
                list.Remove(list.FirstOrDefault(a => a.Name == mod.Name));
            }
            list.Add(mod);
            InstalledMods = list.ToArray();
            Save();
        }
        public void RemoveInstalled(ModDetails mod)
        {
            var list = InstalledMods.ToList();
            list.Remove(list.FirstOrDefault(a => a.Name == mod.Name));
            InstalledMods = list.ToArray();
            Save();
        }
        public void Save()
        {
            
            if (!Directory.Exists(Environment.GetEnvironmentVariable("appdata") + @"\ModFinderWOTR"))
            {
                new DirectoryInfo(Environment.GetEnvironmentVariable("appdata") + @"\ModFinderWOTR\").Create();
            }
            var filepath = (Environment.GetEnvironmentVariable("appdata") + @"\ModFinderWOTR") + @"\Settings.json";

            //var filepath = new DirectoryInfo(Environment.GetEnvironmentVariable("appdata")).FullName+@"\ModFinderWOTR\Settings.json";
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            //Api key encryption
            if(this.NexusAPIKey != null && this.NexusAPIKey != "")
            {
                this.NexusAPIKey = Encrypt(NexusAPIKey);
            }
            if (this.GithubAPIKey != null && this.GithubAPIKey != "")
            {
                this.GithubAPIKey = Encrypt(GithubAPIKey);
            }
            using (StreamWriter sw = new StreamWriter(filepath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, this);
            }
        }
        public static AppSettingsData Load()
        {
            var filepath = new DirectoryInfo(Environment.GetEnvironmentVariable("appdata")).FullName + @"\ModFinderWOTR\Settings.json";
            if (File.Exists(filepath))
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader sr = new StreamReader(filepath))
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    var result = (serializer.Deserialize<AppSettingsData>(reader));
                    if(result.NexusAPIKey != null)
                    result.NexusAPIKey = Unencrypt(result.NexusAPIKey);
                    if (result.GithubAPIKey != null)
                        result.GithubAPIKey = Unencrypt(result.GithubAPIKey);
                    return result;
                }
            }
            else
            {
                return new AppSettingsData();
            }
        }
        public static string Encrypt(string input)
        {
            return input;
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException();
            }
            byte[] rawData = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(ProtectedData.Protect(rawData, null, DataProtectionScope.LocalMachine));
        }
        public static string Unencrypt(string input)
        {
            return input;
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException();
            }
            byte[] rawData = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(ProtectedData.Unprotect(rawData, null, DataProtectionScope.CurrentUser));
        }
    }
}
