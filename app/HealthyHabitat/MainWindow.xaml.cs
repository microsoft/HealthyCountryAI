using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HealthyHabitat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void season_OnDragEnter(object sender, DragEventArgs e)
        {
            if(sender is Path)
            {
                //var fade = new DoubleAnimation
                //{
                //    To = 0,
                //    BeginTime = TimeSpan.FromSeconds(5),
                //    Duration = TimeSpan.FromSeconds(2),
                //    FillBehavior = FillBehavior.Stop
                //};

                //fade.Completed += (s, a) => ((Path)sender).Opacity = 100;

                //((Path)sender).BeginAnimation(UIElement.OpacityProperty, fade);
                ((Path)sender).Opacity = 100;
            }

            base.OnDragEnter(e);
            // Make visable
        }

        private void season_OnDragLeave(object sender, DragEventArgs e)
        {
            if (sender is Path)
            {
                ((Path)sender).Opacity = 0;
            }

            base.OnDragLeave(e);
            // Make invisable
        }

        private void season_OnDragOver(object sender, DragEventArgs e)
        {
            base.OnDragOver(e);
            // Make visable
        }

        private void season_OnDrop(object sender, DragEventArgs e)
        {
            string Season = string.Empty;
            if (sender is Path)
            {
                ((Path)sender).Opacity = 0;
                Season = ((Path)sender).Tag as String; // Not safe
            }

            // Make Propmt for location

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                CopyFilesToLocalCache(files, "cannon-hill", Season);
                // Handle the file move and initiate az-copy task to blog storage

                InitiateAzCopy(); // This needs some sort of callback
            }

            base.OnDrop(e);
        }

        private void CopyFilesToLocalCache(string[] files, string location, string season)
        {
            string cacheLocation = ConfigurationSettings.AppSettings["localCache"] ?? AppDomain.CurrentDomain.BaseDirectory;

            foreach (string file in files)
            {
                DateTime lastModifiedDate = System.IO.File.GetLastWriteTime(file);

                string destination = System.IO.Path.Combine(cacheLocation, string.Format("{0}-{1}", location, season), lastModifiedDate.ToString("yyyy'-'MM'-'dd'-'HH''mm"));

                System.IO.Directory.CreateDirectory(destination);

                string destinationFile = System.IO.Path.Combine(destination, System.IO.Path.GetFileName(file));
                
                System.IO.File.Copy(file, destinationFile, true);
            }
        }

        private void InitiateAzCopy()
        {
            StringBuilder command = new StringBuilder("azcopy cp \"[sourceDir]\" \"https://[account].blob.core.windows.net/[container]?[SAS]\" --recursive=true --put-md5");
            string sourceDir = ConfigurationSettings.AppSettings["localCache"] ?? AppDomain.CurrentDomain.BaseDirectory;
            string account = ConfigurationSettings.AppSettings["storageAccountName"];
            string container = ConfigurationSettings.AppSettings["containerNamwe"];
            string sas = ConfigurationSettings.AppSettings["sas"];

            command.Replace("[sourceDir]", sourceDir);
            command.Replace("[account]", account);
            command.Replace("[container]", container);
            command.Replace("[sas]", sas);

            string location = "cd ./AzCopy";

            using (Process cmd = new Process())
            {
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = false;
                cmd.StartInfo.UseShellExecute = false;

                cmd.Start();
                cmd.StandardInput.WriteLine(location);
                cmd.StandardInput.WriteLine(command);
            }
        }

        private void Btn_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the event to move the upload control around the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
