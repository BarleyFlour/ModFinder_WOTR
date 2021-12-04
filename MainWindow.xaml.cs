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
            Infrastructure.Main.OwlcatEnabledMods = new Infrastructure.OwlcatModificationSettingsManager();
            Infrastructure.Main.OwlcatEnabledMods.Load();
            foreach(var mod in (Infrastructure.Main.AllMods.Where(a => !Infrastructure.Main.Settings.InstalledMods.Any(b => b.Name == a.Name))))
            {
                installedModList.Items.Add(mod);
            }
            foreach(var installedmod in Infrastructure.Main.Settings.InstalledMods)
            {
                installedModList.Items.Add(installedmod);
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
                            if (mod.Name == "Info.json")
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
                //throw new NotImplementedException("InstalledMods");
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
            closeButton.Click += (sender, e) =>
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
                              Debug.WriteLine("[MedFinder]   got latest release (tag): " + latest.TagName);
                              await Dispatcher.InvokeAsync(() =>
                              {
                                  mod.Description = repo.Description;
                                  mod.LatestVersion = latest.TagName.StripV(); //This is not true???
                                  Debug.WriteLine($"setting mod version to: {mod.LatestVersion}");
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

        private void ShowInstalledToggle_Click(object sender, RoutedEventArgs e)
        {
           // this.installedModList.ShowInstalled = !this.installedModList.ShowInstalled;
            //Debug.WriteLine(((ToggleButton)sender).IsChecked);
            var togglebutton = sender as ToggleButton;
            if(togglebutton.IsChecked == false)
            {
                this.installedModList.Items.Clear();
                foreach(var mod in Infrastructure.Main.AllMods.Where(a => !Infrastructure.Main.Settings.InstalledMods.Any(b => b.Name == a.Name)).Concat(Infrastructure.Main.Settings.InstalledMods))
                {
                    this.installedModList.Items.Add(mod);
                }
            }
            else if(togglebutton.IsChecked == true)
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
            if (current == null || current == "")
                return false;
            if (reference == null || reference == "")
                return true;
            try
            {
                int[] thisComps = current.Split('.').Select(s => int.Parse(s)).ToArray();
                int[] referenceComps = reference.Split('.').Select(s => int.Parse(s)).ToArray();

                if (thisComps.Length != referenceComps.Length)
                    return false;

                int a = 0;
                int b = 0;

                for (int i = 0; i < thisComps.Length; i++)
                {
                    int index = thisComps.Length - i - 1;
                    a += (int)(Math.Pow(100, i) * thisComps[index]);
                    b += (int)(Math.Pow(100, i) * referenceComps[index]);
                }

                return a > b;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class ModDetails : INotifyPropertyChanged
    {
        public string Name { get; set; }
        private string _Description;
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
        [JsonIgnore] public bool CanInstall => LatestVersion?.IsLaterThan(InstalledVersion) ?? false;

        private string _InstalledVersion;
        
        public string InstalledVersion
        {
            //get => _InstalledVersion;
            get
            {
                if (_InstalledVersion == null || _InstalledVersion == "")
                {
                    //   return LatestVersion;
                    return _InstalledVersion;
                }
                else return _InstalledVersion;
            }
            set
            {
                _InstalledVersion = value;
                PropertyChanged?.Invoke(this, new(nameof(InstalledVersion)));
                PropertyChanged?.Invoke(this, new(nameof(CanInstall)));
                PropertyChanged?.Invoke(this, new(nameof(InstallButtonText)));
            }
        }

        [JsonIgnore]private string _LatestVersion;
        [JsonIgnore]
        public string LatestVersion
        {
            get => _LatestVersion ?? "...";
            set {
                _LatestVersion = value;
                PropertyChanged?.Invoke(this, new(nameof(LatestVersion)));
                PropertyChanged?.Invoke(this, new(nameof(CanInstall)));
                PropertyChanged?.Invoke(this, new(nameof(InstallButtonText)));
            }
        }

        [JsonIgnore]public string InstallButtonText => CanInstall ? LatestVersion : "up to date";

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
