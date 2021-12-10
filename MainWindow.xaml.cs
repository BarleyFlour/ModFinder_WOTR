using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using IdentityModel.Client;
using System.Threading;

namespace ModFinder_WOTR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow instance;
        public ModListData installedModList = new();
        public MainWindow()
        {
            InitializeComponent();
            instance = this;
            //Infrastructure.ModListLoader.GetModsManifests();
            //Git auth
            {
                Debug.WriteLine(Infrastructure.Main.Client.Credentials.ToString());
                var task = new Task(async () =>
                {

                    {
                        if (Infrastructure.Main.Settings?.GithubAPIKey != null && Infrastructure.Main.Settings?.GithubAPIKey != "")
                        {
                            Infrastructure.Main.Client.Credentials = new Credentials(Infrastructure.Main.Settings.GithubAPIKey);
                            TheRest();
                        }
                        else
                        {
                            var response = await Infrastructure.Main.HTTPClient.RequestDeviceAuthorizationAsync(new DeviceAuthorizationRequest
                            {

                                Address = "https://github.com/login/device/code",

                                ClientId = "d41a1199fe6c315f6949"//,

                                //Parameters =
                                // {
                                // { "scope", "public_dictionary" }
                                //  }
                            });
                            Debug.WriteLine("Stuf" + response.UserCode);

                            var response2 = await Infrastructure.Main.HTTPClient.RequestDeviceTokenAsync(new DeviceTokenRequest
                            {
                                Address = "https://github.com/login/oauth/access_token",
                                ClientId = "d41a1199fe6c315f6949",
                                DeviceCode = response.DeviceCode,
                            });



                            Debug.WriteLine("Stuff " + response2.Error);
                            while (response2.Error == "authorization_pending")
                            {
                                Thread.Sleep(response.Interval * 1000);
                                response2 = await Infrastructure.Main.HTTPClient.RequestDeviceTokenAsync(new DeviceTokenRequest
                                {
                                    Address = "https://github.com/login/oauth/access_token",
                                    ClientId = "d41a1199fe6c315f6949",
                                    DeviceCode = response.DeviceCode,
                                });
                            }
                            Debug.WriteLine("GotToken " + response2.AccessToken);
                            Infrastructure.Main.Client.Credentials = new Credentials(response2.AccessToken);
                            Infrastructure.Main.Settings.GithubAPIKey = response2.AccessToken;
                            Infrastructure.Main.Settings.Save();
                            TheRest();
                        }

                    }
                    /* var clientid = "d41a1199fe6c315f6949";

                     KeyValuePair<string, string> cliid = new(key: "client_id", value: clientid);
                     KeyValuePair<string, string> scope = new(key: "scope", value: "public_repo");
                     var stringContent = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                     {
                         cliid, scope
                     });


                     var loginreq = new OauthLoginRequest(clientid);
                     var asdd = m_Client.Oauth.GetGitHubLoginUrl(loginreq);
                     var result = await HTTPClient.PostAsync("https://github.com/login/device/code", stringContent);
                     var toopen = asdd.ToString().Replace("&", "^&");
                     Process.Start(new ProcessStartInfo("cmd", $"/c start {"https://github.com/login/device"}") { CreateNoWindow = true });
                     JsonSerializer serializer = new JsonSerializer();
                     using (StreamReader sr = new StreamReader(result.Content.ReadAsStream()))
                     using (JsonTextReader reader = new JsonTextReader(sr))
                     {
                         var resultt = serializer.Deserialize<Response>(reader);
                         Debug.WriteLine("Stufufuf" + resultt.user_code);
                         Debug.WriteLine("Stufufuf" + sr.ReadToEnd());
                         KeyValuePair<string, string> device_code = new(key: "device_code", value: resultt.device_code);
                         KeyValuePair<string, string> grant_type = new(key: "grant_type", value: "urn:ietf:params:oauth:grant-type:device_code");
                         var stringContenttwo = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                         {
                             cliid, device_code,grant_type
                         });
                         var rawtokendata = await HTTPClient.PostAsync("https://github.com/login/oauth/access_token", stringContenttwo);
                         while ((await (rawtokendata.Content.ReadAsStringAsync())).Contains("authorization_pending"))
                         {
                             Thread.Sleep(resultt.interval * 1000);
                             Debug.WriteLine("[ModFinder] " + rawtokendata.ReasonPhrase);
                             rawtokendata = await HTTPClient.PostAsync("https://github.com/login/oauth/access_token", stringContenttwo);
                         }
                         JsonSerializer serializer2 = new JsonSerializer();
                         using (StreamReader sr2 = new StreamReader(rawtokendata.Content.ReadAsStream()))
                         using (JsonTextReader reader2 = new JsonTextReader(sr2))
                         {
                             Debug.WriteLine("123 " + sr2.ReadToEnd());
                             var tokendata = serializer2.Deserialize<Response2>(reader2);
                             Debug.WriteLine("GotToken" + tokendata.ToString());
                             m_Client.Credentials = new Credentials(tokendata.access_token);
                         }
                     }
                    */
                });
                task.RunSynchronously();
            }
            void TheRest()
            {
                Infrastructure.Main.OwlcatEnabledMods = new Infrastructure.OwlcatModificationSettingsManager();
                Infrastructure.Main.OwlcatEnabledMods.Load();
                foreach (var mod in (Infrastructure.Main.AllMods.Where(a => !Infrastructure.Main.Settings.InstalledMods.Any(b => b.Name == a.Name))))
                {
                    installedModList.Items.Add(mod);
                }
                foreach (var installedmod in Infrastructure.Main.Settings.InstalledMods)
                {
                    installedModList.Items.Add(installedmod);
                }
                //Nexus & Github API Key Pop-up
                {
                    if (Infrastructure.Main.Settings.GithubAPIKey is null or "" || Infrastructure.Main.Settings.NexusAPIKey is null or "")
                    {
                        //Do bubble popup magic here
                    }
                }
                //Detect currently installed mods
                {
                    //Owlcat mods
                    {
                        foreach (var modfolder in new DirectoryInfo(Infrastructure.Main.PFWotrAppdataPath + @"\Modifications").EnumerateDirectories())
                        {
                            foreach (var mod in modfolder.EnumerateFiles())
                            {
                                if (mod.Name == "OwlcatModificationManifest.json")
                                {
                                    JsonSerializer serializer = new JsonSerializer();
                                    using (StreamReader sr = new StreamReader(mod.FullName))
                                    using (JsonTextReader reader = new JsonTextReader(sr))
                                    {
                                        var result = (serializer.Deserialize<Infrastructure.Loader.OwlcatModificationManifest>(reader));
                                        var newmodinfo = Infrastructure.Main.AllMods.FirstOrDefault(a => a.Name == result.DisplayName);
                                        if (newmodinfo != null && !Infrastructure.Main.Settings.InstalledMods.Any(a => a.Name == newmodinfo.Name))
                                        {
                                            newmodinfo.InstalledVersion = result.Version.StripV();
                                            Infrastructure.Main.Settings.AddInstalled(newmodinfo);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //UMM Mods
                    {
                        foreach (var modfolder in new DirectoryInfo(Infrastructure.Main.WrathPath + @"\Mods").EnumerateDirectories())
                        {
                            foreach (var mod in modfolder.EnumerateFiles())
                            {
                                if (mod.Name == "Info.json" || mod.Name == "info.json")
                                {
                                    JsonSerializer serializer = new JsonSerializer();
                                    using (StreamReader sr = new StreamReader(mod.FullName))
                                    using (JsonTextReader reader = new JsonTextReader(sr))
                                    {
                                        var result = (serializer.Deserialize<Infrastructure.Loader.ModInfo>(reader));
                                        var newmodinfo = Infrastructure.Main.AllMods.FirstOrDefault(a => a.Name == result.DisplayName);
                                        if (newmodinfo != null && !Infrastructure.Main.Settings.InstalledMods.Any(a => a.Name == newmodinfo.Name))
                                        {
                                            newmodinfo.InstalledVersion = result.Version.StripV();
                                            Infrastructure.Main.Settings.AddInstalled(newmodinfo);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                /*installedModList.Items.Add(new ModDetails
                {
                    Name = "BubbleGuns",
                    InstalledVersion = "0.0.1",
                    ModType = Infrastructure.ModType.UMM,
                    Source = Infrastructure.ModSource.Other,
                });
                installedModList.Items.Add(new ModDetails
                {
                    Name = "RespecModWrath",
                    InstalledVersion = "1.09.1",
                    ModType = Infrastructure.ModType.Owlcat,
                    OwnerAndRepo = new string[2] { "BarleyFlour", "RespecMod" },
                    Source = Infrastructure.ModSource.GitHub,
                });
                installedModList.Items.Add(new ModDetails
                {
                    Name = "Test Mod",
                    InstalledVersion = "1.2.3",
                    // ModType = "UMM",
                    Source = Infrastructure.ModSource.Nexus,
                });*/

                installedMods.DataContext = installedModList;
                showInstalledToggle.DataContext = installedModList;
                showInstalledToggle.Click += ShowInstalledToggle_Click;


                // Do magic window dragging regardless where they click
                MouseDown += (sender, e) =>
                {
                    if (e.ChangedButton == MouseButton.Left)
                        DragMove();
                };

                // Close button
                closeButton.Click += async (sender, e) =>
                {
                    Close();
                };

                // Drag drop nonsense
                dropTarget.Drop += DropTarget_Drop;
                dropTarget.DragOver += DropTarget_DragOver;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        foreach (var mod in installedModList.Items.Concat(Infrastructure.Main.Settings.InstalledMods).Concat(Infrastructure.Main.AllMods))
                        {
                            if (mod.Source == Infrastructure.ModSource.GitHub)
                            {
                                Debug.WriteLine("[ModFinder] ModSourceGitDescrip " + mod.Name + "\n");
                                var repo = await Infrastructure.Main.Client.Repository.Get(mod.OwnerAndRepo[0], mod.OwnerAndRepo[1]);
                                var latest = await Infrastructure.Main.Client.Repository.Release.GetLatest(mod.OwnerAndRepo[0], mod.OwnerAndRepo[1]);
                                Debug.WriteLine("[ModFinder]        got repo name: " + repo.FullName);
                                Debug.WriteLine("[ModFinder] got repo description: " + repo.Description);
                                Debug.WriteLine("[ModFinder]   got latest release (tag): " + latest.TagName);
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    mod.Description = repo.Description;
                                    mod.LatestVersion = latest.TagName.StripV(); //This is not true???
                                    Debug.WriteLine($"setting mod version to: {mod.LatestVersion}");
                                    Debug.WriteLine($"can install: {mod.CanInstall}");
                                });
                            }
                            else if (mod.Source == Infrastructure.ModSource.Nexus)
                            {
                                // Debug.WriteLine(mod.NexusModID);
                                var nexusmod = await NexusModsNET.NexusModsFactory.New(Infrastructure.Main.NexusClient).CreateModsInquirer().GetMod("pathfinderwrathoftherighteous", long.Parse(mod.NexusModID));

                                await Dispatcher.InvokeAsync(() =>
                                {
                                    
                                    mod.Description = nexusmod.Description;
                                    mod.LatestVersion = nexusmod.Version.StripV(); //This is not true???
                                    Debug.WriteLine("Stuff " + mod.Name + $"setting mod version to: {mod.LatestVersion}");
                                    Debug.WriteLine($"can install: {mod.CanInstall}");
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
            }
        }
        private void ShowInstalledToggle_Click(object sender, RoutedEventArgs e)
        {
            // this.installedModList.ShowInstalled = !this.installedModList.ShowInstalled;
            //Debug.WriteLine(((ToggleButton)sender).IsChecked);
            var togglebutton = sender as ToggleButton;
            if (togglebutton.IsChecked == false)
            {
                this.installedModList.Items.Clear();
                foreach (var mod in Infrastructure.Main.AllMods.Where(a => !Infrastructure.Main.Settings.InstalledMods.Any(b => b.Name == a.Name)).Concat(Infrastructure.Main.Settings.InstalledMods))
                {
                    this.installedModList.Items.Add(mod);
                }
            }
            else if (togglebutton.IsChecked == true)
            {
                this.installedModList.Items.Clear();
                foreach (var mod in Infrastructure.Main.Settings.InstalledMods)
                {
                    this.installedModList.Items.Add(mod);
                }
            }
        }

        private void DropTarget_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (e.Data.GetFormats().Any(f => f == DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.All(f => CheckIsMod(f)))
                {
                    e.Effects = DragDropEffects.Copy;
                }
            }
            e.Handled = true;
        }

        private void DropTarget_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string f in files)
            {
                Infrastructure.Loader.ModInstall.InstallFromZip(f);
                //BARLEY CODE HERE TO INSTALL FILE
            }
        }

        public static bool CheckIsMod(string path)
        {
            if (!File.Exists(path))
                return false;

            if (System.IO.Path.GetExtension(path) != ".zip")
                return false;

            //BARLEY CODE HERE
            using ZipArchive opened = ZipFile.OpenRead(path);
            return opened.Entries.Any(a => a.Name is @"OwlcatModificationManifest.json" or @"Info.json");
        }

        private void InstallOrUpdateMod(object sender, RoutedEventArgs e)
        {
            ModDetails toInstall = (sender as Button).Tag as ModDetails;
            Infrastructure.Loader.ModInstall.InstallFromManagerGit(toInstall);
            //BARLEY CODE HERE
        }
    }

    public class ModListData : INotifyPropertyChanged
    {
        private bool _ShowInstalled;
        public bool ShowInstalled
        {
            get => _ShowInstalled;
            set
            {
                _ShowInstalled = value;
                PropertyChanged?.Invoke(this, new(nameof(ShowInstalled)));
                PropertyChanged?.Invoke(this, new(nameof(HeaderNameText)));
            }
        }
        public ObservableCollection<ModDetails> Items { get; set; } = new();
        public string HeaderNameText => ShowInstalled ? "Update" : "Install";

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public static class Versions
    {
        public static string StripV(this string version)
        {
            if (version.Length > 0 && (version[0] == 'v' || version[0] == 'V'))
                return version[1..];
            else
                return version;

        }
        public static bool IsLaterThan(this string current, string reference)
        {
            if (current == null || current == "" || current == "...")
                return false;
            if (reference == null || reference == "" || reference == "...")
                return true;
           // try
            {
                Debug.WriteLine("Shtuffs");

                var filtered_current = new string(current.Where(a => char.IsDigit(a) || char.IsNumber(a) || a == '.' || a == '_').ToArray());
                var filtered_reference = new string(current.Where(a => char.IsDigit(a) || char.IsNumber(a) || a == '.' || a == '_').ToArray());

                int[] thisComps = null;
                int[] referenceComps = null;
                if (filtered_current.Any(a => a == '.'))
                {
                    thisComps = filtered_current.Split('.').Select(s => int.Parse(s)).ToArray();
                }
                else if (filtered_current.Any(a => a == '_'))
                {
                    thisComps = filtered_current.Split('_').Where(x => x != "").Select(s => int.Parse(s)).ToArray();
                }
                if (filtered_reference.Any(a => a == '.'))
                {
                    referenceComps = filtered_reference.Split('.').Select(s => int.Parse(s)).ToArray();
                }
                else if (filtered_reference.Any(a => a == '_'))
                {
                    referenceComps = filtered_reference.Split('_').Where(x => x != "").Select(s => int.Parse(s)).ToArray();
                }
                //  if (thisComps.Length != referenceComps.Length)
                //  return false;



                //Alternate Barley Magic
                {
                    int y = 1000;
                    Debug.WriteLine("Stuffs");
                    int thiscompnum = 0;
                    int refcompnum = 0;
                    for (int i = 0; i <= thisComps.Length - 1; i++)
                    {
                        var preprocess = thisComps[i];
                        if (preprocess.ToString().Length > 1)
                        {
                            Debug.WriteLine("longer " + preprocess);
                            preprocess = (int)((preprocess * 0.1) * y);
                            Debug.WriteLine(preprocess);
                        }
                        else
                        {
                            preprocess = preprocess * y;
                        }
                        y = y / 10;
                        thiscompnum += preprocess;
                        //  refcompnum += referenceComps[i].ToString();
                    }
                    int z = 1000;
                    for (int i = 0; i <= referenceComps.Length - 1; i++)
                    {
                        var preprocess = referenceComps[i];
                        if (preprocess.ToString().Length > 1)
                        {
                            preprocess = (int)MathF.Pow((float)(preprocess * 0.1), (float)preprocess.ToString().Length);
                        }
                        var postprocess = preprocess * z;
                        z = z / 10;
                        refcompnum += postprocess;
                        //  refcompnum += referenceComps[i].ToString();
                    }
                    Debug.WriteLine("ThisSum " + thiscompnum);
                    Debug.WriteLine("RefSum " + refcompnum );
                    return thiscompnum > refcompnum;

                }


                int a = 0;
                int b = 0;
                for (int i = 0; i < thisComps.Length; i++)
                {
                    int index = thisComps.Length - i - 1;
                    a += (int)(Math.Pow(100, i) * thisComps[index]);
                    double v = (Math.Pow(100, i) * referenceComps[index]);
                    b += (int)v;
                }

                return a > b;
            }
            //catch (Exception e)
            {
              //  Debug.WriteLine(e.ToString());
              //  return false;
            }
        }
    }

    public class ModDetails : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Author { get; set; }
        private string _Description;
        [JsonIgnore]
        public string Description
        {
            get => _Description ?? "Loading...";
            set
            {
                _Description = value;
                PropertyChanged?.Invoke(this, new(nameof(Description)));
            }

        }
        public Infrastructure.ModType ModType { get; set; }
        public Infrastructure.ModSource Source { get; set; }
        //Formatted as §RepoOwner§/§RepoName§
        public string[] OwnerAndRepo { get; set; }

        //BARLEY CODE HERE
        [JsonIgnore]
        private bool m_CanInstall;
        [JsonIgnore]
        public bool CanInstall
        {
            get
            {
               // if (m_CanInstall == null)
                {
                    if (_InstalledVersion != null && _InstalledVersion != "") m_CanInstall = LatestVersion?.IsLaterThan(InstalledVersion) ?? false;
                    else m_CanInstall = true;
                    return m_CanInstall; 
                }
              //  return (bool)m_CanInstall;
            }
        }

        private string _InstalledVersion;

        public string InstalledVersion
        {
            //get => _InstalledVersion;
            get
            {
                /*if (_InstalledVersion == null || _InstalledVersion == "")
                {
                    return _InstalledVersion;
                    //return _InstalledVersion;
                }*/
                /*else*/ return _InstalledVersion;
            }
            set
            {
                _InstalledVersion = value;
                PropertyChanged?.Invoke(this, new(nameof(InstalledVersion)));
                PropertyChanged?.Invoke(this, new(nameof(CanInstall)));
                PropertyChanged?.Invoke(this, new(nameof(InstallButtonText)));
            }
        }

        private string _LatestVersion;
        [JsonIgnore]
        public string LatestVersion
        {
            get => _LatestVersion ?? "...";
            set
            {
                _LatestVersion = value;
                PropertyChanged?.Invoke(this, new(nameof(LatestVersion)));
                PropertyChanged?.Invoke(this, new(nameof(CanInstall)));
                PropertyChanged?.Invoke(this, new(nameof(InstallButtonText)));
            }
        }

        [JsonIgnore] public string InstallButtonText => CanInstall ? LatestVersion : "up to date";
        public string NexusModID;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
