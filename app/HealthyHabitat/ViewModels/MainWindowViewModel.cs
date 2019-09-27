namespace HealthyHabitat.ViewModels
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using MvvmDialogs;
    using SixSeasons.Helpers;
    using SixSeasons.Models;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    public class MainWindowViewModel : ViewModelBase
    {
        #region Commands
        public RelayCommand CopyCommand
        {
            get;
            private set;
        }

        public RelayCommand ExitCommand
        {
            get;
            private set;
        }

        public RelayCommand UploadClickCommand
        {
            get;
            private set;
        }

        public RelayCommand<DragEventArgs> DropCommand
        {
            get;
            private set;
        }

        public ICommand ShowMessageBoxWithMessageCommand { get; }
        #endregion Commands

        private readonly IDialogService dialogService;
        //private string confirmation;

        private string _selectedSeason;

        public string SelectedSeason
        {
            get { return _selectedSeason; }
            set { _selectedSeason = value; }
        }
        private ObservableCollection<string> _files;

        public ObservableCollection<string> Files
        {
            get { return _files; }
            set { _files = value; }
        }

        private ObservableCollection<Location> _locations;

        public ObservableCollection<Location> Locations
        {
            get { return _locations; }
            set { _locations = value; }
        }

        private Location _selectedLocation;

        public Location SelectedLocation
        {
            get { return _selectedLocation; }
            set { _selectedLocation = value; }
        }

        //        private readonly BackgroundWorker worker;

        private double _progressValueMax;
        public double ProgressValueMax
        {
            get { return _progressValueMax; }
            set
            {
                _progressValueMax = value;
                RaisePropertyChanged("ProgressValueMax");
            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                RaisePropertyChanged("ProgressValue");
                RaisePropertyChanged("ProgressText");
            }
        }

        private bool _progressVisible;
        public bool ProgressVisible
        {
            get { return _progressVisible; }
            set
            {
                _progressVisible = value;
                RaisePropertyChanged("ProgressVisible");
            }
        }

        public string ProgressText
        {
            get { return string.Format("{0} %", _progressValue); }
        }

        private bool _locationDialogVisible;
        public bool LocationDialogVisible
        {
            get { return _locationDialogVisible; }
            set
            {
                _locationDialogVisible = value;
                RaisePropertyChanged("LocationDialogVisible");
            }
        }

        public MainWindowViewModel(IDialogService dialogService)
        {
            NameValueCollection locationSection = (NameValueCollection)ConfigurationManager.GetSection("locations");

            Locations = new ObservableCollection<Location>();

            foreach (var loc in locationSection.AllKeys)
            {
                Locations.Add(new Location() { Id = loc, Name = locationSection[loc] });
            }

            this.dialogService = dialogService;

            UploadClickCommand = new RelayCommand(() =>
            {
                LocationDialogVisible = false;

                ProgressVisible = true;

                CopyFilesToLocalCache(this.Files.ToArray<string>(), this.SelectedLocation.Id, this.SelectedSeason);
            });

            ExitCommand = new RelayCommand(() =>
            {
                System.Windows.Application.Current.Shutdown();
            });

            DropCommand = new RelayCommand<DragEventArgs>(e =>
            {
                SelectedSeason = ((System.Windows.Shapes.Path)e.Source).Tag.ToString();//Not safe

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    Files = new ObservableCollection<string>((string[])e.Data.GetData(DataFormats.FileDrop));
                }

                LocationDialogVisible = true;
            });
        }

        private async void CopyFilesToLocalCache(string[] files, string location, string season)
        {
            string cacheLocation = System.Configuration.ConfigurationManager.AppSettings["localCache"].ToString() ?? AppDomain.CurrentDomain.BaseDirectory;

            Dictionary<string, string> fileCopyManifest = new Dictionary<string, string>();

            double size = 0;

            foreach (string filename in files)
            {
                DateTime lastModifiedDate = System.IO.File.GetLastWriteTime(filename);

                string destination = System.IO.Path.Combine(cacheLocation, string.Format("{0}-{1}", location, season), lastModifiedDate.ToString("yyyy'-'MM'-'dd'-'HH''mm"));

                System.IO.Directory.CreateDirectory(destination);

                string destinationFile = System.IO.Path.Combine(destination, System.IO.Path.GetFileName(filename));

                fileCopyManifest.Add(filename, destinationFile);
                FileInfo file = new FileInfo(filename);
                size += file.Length;
            }

            ProgressValueMax = size;

            await AsyncFileCopy.CopyFiles(fileCopyManifest, prog => ProgressValue = prog);

            ProgressVisible = false;
        }
    }
}
