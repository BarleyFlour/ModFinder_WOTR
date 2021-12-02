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
                Version = "0.0.1",
                ModType = Infrastructure.ModType.UMM,
                Source = Infrastructure.ModSource.Other,
                CanInstall = false,
            });
            installedModList.Items.Add(new ModDetails
            {
                Name = "RespecModWrath",
                Version = "4.2.0",
                ModType = Infrastructure.ModType.Owlcat,
                OwnerAndRepo = new string[2] { "BarleyFlour", "RespecMod" },
                Source = Infrastructure.ModSource.GitHub,
                CanInstall = true,
            });
            installedModList.Items.Add(new ModDetails
            {
                Name = "Test Mod",
                Version = "1.2.3",
                // ModType = "UMM",
                Source = Infrastructure.ModSource.Nexus,
                CanInstall = false,
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
            {
                var opened = ZipFile.OpenRead(path);
                if (opened.Entries.Any(a => a.Name == @"OwlcatModificationManifest.json" || a.Name == @"Info.json")) return true;
            }

            return false;
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

    public class ModDetails
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description
        {
            get
            {
                // Get From Github/Nexus
                {
                    if (this.Source == Infrastructure.ModSource.GitHub)
                    {
                        Debug.Write(" ModSourceGitDescrip " + this.Name + "\n");
                        using var wc = new WebClient();
                        {
                            Debug.Write(OwnerAndRepo[0] + " " + OwnerAndRepo[1]);
                            /* var descrip = Task.Run(() => 
                             {
                                 var j = Infrastructure.Main.client.Repository.Get(OwnerAndRepo[0], OwnerAndRepo[1]);
                                 return j;

                             });*/
                            var task = Task.Run(() => Infrastructure.Main.client.Repository.Get(OwnerAndRepo[0], OwnerAndRepo[1]));
                            task.ContinueWith(t =>
                            {
                                //do somethign with t.Result here
                                Debug.Write(t.Result.Description);
                                return t.Result.Description;
                            });
                            // else
                            {
                                return "Failed to get GitHub Description";
                            }
                        }
                    }
                    else if (this.Source == Infrastructure.ModSource.Nexus)
                    {
                        if (Infrastructure.Main.Settings.NexusAPIKey == null || Infrastructure.Main.Settings.NexusAPIKey == "")
                        {
                            return "Please log in to nexus to view descriptions (Nexus' API Requires this)";
                        }
                        else
                        {
                            return "Placeholder";
                            //Nexus API stuff here
                        }
                    }
                    else
                    {
                        return "No Description Found";
                    }
                }
            }
        }
        public Infrastructure.ModType ModType { get; set; }
        public Infrastructure.ModSource Source { get; set; }
        //Formatted as §RepoOwner§/§RepoName§
        public string[] OwnerAndRepo { get; set; }

        //BARLEY CODE HERE
        public bool CanInstall { get; set; }

        public string InstallButtonText => CanInstall ? "v0.06.11" : "up to date";

    }
}
