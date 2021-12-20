using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ModFinder_WOTR;
using ModFinder_WOTR.Infrastructure;
using Octokit;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO.Compression;
using ManifestUpdater.Properties;
using NexusModsNET;

namespace ManifestUpdater
{
    class Program
    {
        private static INexusModsClient m_NexusClient;
        public static INexusModsClient NexusClient => m_NexusClient ??= NexusModsClient.Create(Environment.GetEnvironmentVariable("NEXUS_APITOKEN"), "Modfinder_WOTR", "0");
        private static GitHubClient m_Client;
        public static GitHubClient Client
        {
            get
            {
                if (m_Client == null)
                {
                    m_Client = new(new ProductHeaderValue("ModFinder_WOTR"));
                    m_Client.Credentials = new(Environment.GetEnvironmentVariable("GITHUB_TOKEN"));
                }
                return m_Client;
            }
        }
        static void Log(string line)
        {
            Console.Write("[ModFinder]  ");
            Console.WriteLine(line);
        }

        public class ProcessedModDetails
        {
            public string FullName;
            public string ReleaseTag;

            public ModDetailsInternal Details;
        }

        public static void Main(string[] _)
        {
            var details = ModFinderIO.FromString<ModListBlob>(Resources.Master_Manifest);

            var tasks = new List<Task<ProcessedModDetails>>();
            foreach (var mod in details.m_AllMods)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var details = new ProcessedModDetails()
                    {
                        Details = mod,
                    };
                    if (mod.Source == ModSource.GitHub)
                    {
                        Console.WriteLine("[ModFinder] ModSourceGitDescrip " + mod.Name + "\n");
                        var repo = await Client.Repository.Get(mod.GithubOwner, mod.GithubRepo);
                        var latest = await Client.Repository.Release.GetLatest(mod.GithubOwner, mod.GithubRepo);
                        var todownload = latest.Assets[0];

                        details.FullName = repo.FullName;
                        details.ReleaseTag = latest.TagName;

                        mod.DownloadLink = todownload.BrowserDownloadUrl;
                        var readme = await Client.Repository.Content.GetReadme(mod.GithubOwner, mod.GithubRepo);
                        if (readme != null)
                            mod.Description = readme.Content;
                        else
                            mod.Description = repo.Description;

                    }
                    else if (mod.Source == ModSource.Nexus)
                    {
                        var nexusmod = await NexusModsFactory.New(NexusClient).CreateModsInquirer().GetMod("pathfinderwrathoftherighteous", mod.NexusModID);
                        var modde = await NexusModsFactory.New(NexusClient).CreateModFilesInquirer().GetModFilesAsync("pathfinderwrathoftherighteous", mod.NexusModID);
                        var release = modde.ModFiles.Last();

                        details.FullName = nexusmod.Name;
                        details.ReleaseTag = nexusmod.Version;

                        mod.Description = nexusmod.Description;
                        mod.DownloadLink = @"https://www.nexusmods.com/pathfinderwrathoftherighteous/mods/" + mod.NexusModID + @"?tab=files&file_id=" + release.FileId;
                    }

                    mod.Latest = ModVersion.Parse(details.ReleaseTag);
                    return details;
                }));
            }

            details.m_AllMods = new();
            foreach (var result in tasks.Select(t => t.Result))
            {
                details.m_AllMods.Add(result.Details);

                Log(result.Details.Name);
                Log("    FullName:".PadRight(20) + result.FullName);
                Log("    ReleaseTag:".PadRight(20) + result.ReleaseTag);
                Log("         Version:".PadRight(20) + result.Details.Latest);
                Log("            Id:".PadRight(20) + result.Details.ModId);
            }

            ModFinderIO.Write(details, Environment.GetEnvironmentVariable("MODFINDER_LOCAL_MANIFEST"));

                //Push stuff here
                //var j = new UpdateFileRequest("Automatic Update",,);
               // Client.Repository.Content.UpdateFile();
            Console.WriteLine("Hello World!");
        }
    }
}
