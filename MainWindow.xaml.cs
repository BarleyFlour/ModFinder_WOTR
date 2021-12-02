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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModFinder_WOTR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModListData installedModList = new();

        public MainWindow()
        {
            InitializeComponent();
            //Infrastructure.ModListLoader.GetModsManifests();
            Infrastructure.Main.OwlcatEnabledMods = new Infrastructure.OwlcatModificationSettingsManager();
            Infrastructure.Main.OwlcatEnabledMods.Load();



            installedModList.Items.Add(new ModDetails
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
            });

            installedMods.DataContext = installedModList;
            showInstalledToggle.DataContext = installedModList;


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
                      foreach (var mod in installedModList.Items)
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
            if (current == null || reference == null)
                return false;

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
        public bool CanInstall => LatestVersion?.IsLaterThan(InstalledVersion) ?? false;

        private string _InstalledVersion;
        public string InstalledVersion
        {
            get => _InstalledVersion;
            set
            {
                _InstalledVersion = value;
                PropertyChanged?.Invoke(this, new(nameof(InstalledVersion)));
                PropertyChanged?.Invoke(this, new(nameof(CanInstall)));
                PropertyChanged?.Invoke(this, new(nameof(InstallButtonText)));
            }
        }

        private string _LatestVersion;
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

        public string InstallButtonText => CanInstall ? LatestVersion : "up to date";

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
