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
        public string[] EnabledModifications = new string[0];
        public string NexusAPIKey;
        public void Save()
        {
            var filepath = new DirectoryInfo(Environment.GetEnvironmentVariable("appdata")).Parent.FullName+@"\LocalLow\ModFinderWOTR";
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            //Api key encryption
            {
                this.NexusAPIKey = Encrypt(NexusAPIKey);
            }
            using (StreamWriter sw = new StreamWriter(filepath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, this);
            }
        }
        public static AppSettingsData Load()
        {
            var filepath = Path.Combine(Main.PFWotrAppdataPath.FullName, "OwlcatModificationManangerSettings.json");
            if (File.Exists(filepath))
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader sr = new StreamReader(filepath))
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    var result = (serializer.Deserialize<AppSettingsData>(reader));
                    if(result.NexusAPIKey != null)
                    result.NexusAPIKey = Unencrypt(result.NexusAPIKey);
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
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException();
            }
            byte[] rawData = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(ProtectedData.Protect(rawData, null, DataProtectionScope.LocalMachine));
        }
        public static string Unencrypt(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException();
            }
            byte[] rawData = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(ProtectedData.Unprotect(rawData, null, DataProtectionScope.LocalMachine));
        }
    }
}
