using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModFinder_WOTR.Infrastructure
{
    public class SettingsData
    {
        [JsonProperty]
        public string[] EnabledModifications = new string[0];
    }
    public class OwlcatModificationSettingsManager
    {
        public SettingsData EnabledModifications;
        public void Remove(string uniqueid)
        {
            if (EnabledModifications.EnabledModifications.Any(a => a == uniqueid))
            {
                var list = EnabledModifications.EnabledModifications.ToList();
                list.Remove(uniqueid);
                EnabledModifications.EnabledModifications = list.ToArray();
                this.Save();
            }
        }
        public void Add(string uniqueid)
        {
            if (!EnabledModifications.EnabledModifications.Any(a => a == uniqueid))
            {
                var list = EnabledModifications.EnabledModifications.ToList();
                list.Add(uniqueid);
                EnabledModifications.EnabledModifications = list.ToArray();
                this.Save();
            }
        }
        public void Save()
        {
            var filepath = Path.Combine(Main.PFWotrAppdataPath.FullName, "OwlcatModificationManangerSettings.json");
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            using (StreamWriter sw = new StreamWriter(filepath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, EnabledModifications);
            }
        }
        public void Load()
        {
            var filepath = Path.Combine(Main.PFWotrAppdataPath.FullName, "OwlcatModificationManangerSettings.json");
            if (File.Exists(filepath))
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader sr = new StreamReader(filepath))
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    var result = (serializer.Deserialize<SettingsData>(reader));
                    this.EnabledModifications = result;
                }
            }
        }
    }
}