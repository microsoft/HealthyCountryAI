namespace HealthyHabitat.ViewModels
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using MvvmDialogs;
    using SixSeasons.Models;
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
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

        private readonly BackgroundWorker worker;
        private int _progressValue;
        public int ProgressValue
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

            //Locations = new ObservableCollection<Location>()
            //{
            //    new Location(){ Id="cannon-hill", Name="Cannon Hill" },
            //    new Location(){ Id="jabire-dreaming", Name="Jabire Dreaming" },
            //    new Location(){ Id="ubir", Name="Ubir" }
            //};

            this.dialogService = dialogService;

            //ShowMessageBoxWithMessageCommand = new RelayCommand(ShowMessageBoxWithMessage);

            this.worker = new BackgroundWorker();
            this.worker.WorkerReportsProgress = true;
            //this.worker.DoWork += this.DoWork;
            this.worker.ProgressChanged += this.ProgressChanged;

            CopyCommand = new RelayCommand(() =>
            {
                this.worker.RunWorkerAsync();
            }, () =>
            {
                return !this.worker.IsBusy;
            });

            UploadClickCommand = new RelayCommand(() =>
            {
                LocationDialogVisible = false;

                OpenCopyDialog();
            });

            ExitCommand = new RelayCommand(() =>
            {
                System.Windows.Application.Current.Shutdown();
            });

            DropCommand = new RelayCommand<DragEventArgs>(e =>
            {
                //var element = e.OriginalSource as UIElement;
                //element.Opacity = 0;
                SelectedSeason = ((System.Windows.Shapes.Path)e.Source).Tag.ToString();//Not safe

                // Make Propmt for location

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    Files = new ObservableCollection<string>((string[])e.Data.GetData(DataFormats.FileDrop));
                }

                OpenLocationDialog();
            });
        }

        private void OpenLocationDialog()
        {
            LocationDialogVisible = true;
        }

        private void OpenCopyDialog()
        {
            ProgressVisible = true;

            CopyFilesToLocalCache(this.Files.ToArray<string>(), this.SelectedLocation.Id, this.SelectedSeason);
        }

        private void CopyFilesToLocalCache(string[] files, string location, string season)
        {
            string cacheLocation = System.Configuration.ConfigurationManager.AppSettings["localCache"].ToString() ?? AppDomain.CurrentDomain.BaseDirectory;

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

        }

        //private void DoWork(object sender, DoWorkEventArgs e)
        //{
        //    for (int i = 0; i <= 100; i++)
        //    {
        //        Thread.Sleep(10);//Do your Work Here instead of sleeping
        //        worker.ReportProgress(i);
        //    }
        //}

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.ProgressValue = e.ProgressPercentage;
        }
    }

    //public class CustomFileCopier
    //{
    //    public CustomFileCopier(string Source, string Dest)
    //    {
    //        this.SourceFilePath = Source;
    //        this.DestFilePath = Dest;

    //        OnProgressChanged += delegate { };
    //        OnComplete += delegate { };
    //    }

    //    public void Copy()
    //    {
    //        byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
    //        bool cancelFlag = false;

    //        using (FileStream source = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read))
    //        {
    //            long fileLength = source.Length;
    //            using (FileStream dest = new FileStream(DestFilePath, FileMode.CreateNew, FileAccess.Write))
    //            {
    //                long totalBytes = 0;
    //                int currentBlockSize = 0;

    //                while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
    //                {
    //                    totalBytes += currentBlockSize;
    //                    double persentage = (double)totalBytes * 100.0 / fileLength;

    //                    dest.Write(buffer, 0, currentBlockSize);

    //                    cancelFlag = false;
    //                    OnProgressChanged(persentage, ref cancelFlag);

    //                    if (cancelFlag == true)
    //                    {
    //                        // Delete dest file here
    //                        break;
    //                    }
    //                }
    //            }
    //        }

    //        OnComplete();
    //    }

    //    public string SourceFilePath { get; set; }
    //    public string DestFilePath { get; set; }

    //    //public event ProgressChangeDelegate OnProgressChanged;
    //    //public event Completedelegate OnComplete;
    //}
}
