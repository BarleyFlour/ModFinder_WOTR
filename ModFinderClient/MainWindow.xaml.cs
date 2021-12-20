using Newtonsoft.Json;
using Octokit;
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
using IdentityModel.Client;
using System.Threading;
using ModFinder_WOTR.Infrastructure;
using static ModFinder_WOTR.MainWindow;
using System;

namespace ModFinder_WOTR
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ModDatabase modListData = ModDatabase.Instance;

        public MainWindow()
        {
            InitializeComponent();
            installedMods.DataContext = modListData;
            showInstalledToggle.DataContext = modListData;
            showInstalledToggle.Click += ShowInstalledToggle_Click;

#if DEBUG
            var manifest = JsonConvert.DeserializeObject<ModListBlob>(File.ReadAllText(Environment.GetEnvironmentVariable("MODFINDER_LOCAL_MANIFEST")));
#else
            using var client = new WebClient();
            var manifest = client.DownloadString("https://URL_TO_CONVERTED_MANIFEST_HERE.json");
#endif

            installedMods.SelectedCellsChanged += (sender, e) =>
            {
                if (e.AddedCells.Count > 0)
                    installedMods.SelectedItem = null;
            };

            foreach (var mod in manifest.m_AllMods)
                modListData.Add(new(mod));

            ModInstall.ParseInstalledMods();


            // Do magic window dragging regardless where they click
            MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            };

            LocationChanged += (sender, e) =>
            {
                double offset = DescriptionPopup.HorizontalOffset;
                DescriptionPopup.HorizontalOffset = offset + 1;
                DescriptionPopup.HorizontalOffset = offset;
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


        public static bool CheckIsMod(string path)
        {
            if (!File.Exists(path))
                return false;

            if (System.IO.Path.GetExtension(path) != ".zip")
                return false;

            //BARLEY CODE HERE
            using var opened = ZipFile.OpenRead(path);
            return opened.Entries.Any(a => a.Name.Equals("OwlcatModificationManifest.json", StringComparison.OrdinalIgnoreCase) || a.Name.Equals("Info.json", StringComparison.OrdinalIgnoreCase));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            DescriptionPopup.IsOpen = false;
        }

        private void DataGridRow_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void DataGridRow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("AKLSJDKLajsD");
            DescriptionPopup.IsOpen = false;
        }

        private void DataGridRow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var details = (sender as DataGridRow)?.Item as ModDetails;
            if (details == null)
                return;
            DescriptionPopup.IsOpen = true;
            DescriptionPopup.DataContext = details;
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

        private void ProcessIntallResult(InstallResult result)
        {
            if (result.Complete)
                result.Mod.State = ModState.Installed;
            else if (result.Mod != null)
                result.Mod.State = ModState.NotInstalled;

            if (result.Error != null)
            {
                _ = MessageBox.Show(this, "Could not install mod: " + result.Error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DropTarget_Drop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var f in files)
            {
                var result = await ModInstall.InstallFromZip(f);
                ProcessIntallResult(result);
            }
        }

        private async void InstallOrUpdateMod(object sender, RoutedEventArgs e)
        {
            var toInstall = (sender as Button).Tag as ModDetails;
            toInstall.State = ModState.Installing;

            var result = await ModInstall.InstallMod(toInstall);
            ProcessIntallResult(result);

        }


        private void ShowInstalledToggle_Click(object sender, RoutedEventArgs e)
        {
            var togglebutton = sender as ToggleButton;
            modListData.ShowInstalled = togglebutton.IsChecked ?? false;
        }

        private void MoreOptions_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.ContextMenu.DataContext = button.Tag;
            button.ContextMenu.StaysOpen = true;
            button.ContextMenu.IsOpen = true;

        }

        private void LookButton_Click(object sender, RoutedEventArgs e)
        {
            ModInstall.ParseInstalledMods();
        }
    }
}
