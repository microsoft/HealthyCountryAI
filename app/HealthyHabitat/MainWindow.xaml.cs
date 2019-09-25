using System;
using System.Collections.Generic;
using System.Configuration;
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

                CopyFilesToLocalCache(files, "Camp Hill", Season);
                // Handle the file move and initiate az-copy task to blog storage

                InitiateAzCopy(); // This needs some sort of callback
            }

            base.OnDrop(e);
        }

        private void CopyFilesToLocalCache(string[] files, string location, string season)
        {
            string cacheLocation = ConfigurationSettings.AppSettings["localCache"] ?? AppDomain.CurrentDomain.BaseDirectory;
            string destination = System.IO.Path.Combine(cacheLocation, location, season);

            System.IO.Directory.CreateDirectory(destination);

            foreach (string file in files)
            {
                string destinationFile = System.IO.Path.Combine(destination, System.IO.Path.GetFileName(file));
                System.IO.File.Copy(file, destinationFile, true);
            }
        }

        private void InitiateAzCopy()
        {

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
