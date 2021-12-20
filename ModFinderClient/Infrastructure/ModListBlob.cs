using Newtonsoft.Json;
using System.Collections.Generic;

namespace ModFinder_WOTR.Infrastructure
{
    public class ModListBlob
    {
        [JsonProperty] public List<ModDetailsInternal> m_AllMods;
    }

}
