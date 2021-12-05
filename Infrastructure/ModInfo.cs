using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModFinder_WOTR.Infrastructure
{
    //Mod type, determined by Manifest type, or lack of Manifset
    public enum ModType
    {
        Owlcat = 0,
        UMM = 1,
        Other = 2
    }
    public enum ModSource
    {
        GitHub = 0,
        Nexus = 1,
        Other = 2,
        ModDB = 3
    }
    /*public class ModInfo
    {
        public ModType modtype;
        ///Unique identifier without spaces.
        public string UniqueName;
        ///Name to display (Can have spaces)
        public string DisplayName;
        //Version number, checked against latest release
        public string Version;
        public ModInfo(ModType type, string uniquename, string displayname, string version)
        {
            this.modtype = type;
            this.UniqueName = uniquename;
            this.DisplayName = displayname;
            this.Version = version;
        }
        public ModInfo()
        {
        }
    }
    public class InstalledModInfo
    {
        public ModType modtype;
        ///Unique identifier without spaces.
        public string UniqueName;
        ///Name to display (Can have spaces)
        public string DisplayName;
        //Version number, checked against latest release
        public InstalledModInfo(ModType type, string uniquename, string displayname)
        {
            this.modtype = type;
            this.UniqueName = uniquename;
            this.DisplayName = displayname;
        }
        public InstalledModInfo()
        {
        }
    }*/
}
