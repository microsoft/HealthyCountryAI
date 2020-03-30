namespace HealthyHabitat.ViewModels
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using HealthyHabitat.Helpers;
    using HealthyHabitat.Models;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Blob;
    using MvvmDialogs;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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

        public RelayCommand<DragEventArgs> DropCommand
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

        public ICommand ShowMessageBoxWithMessageCommand { get; }

        #endregion Commands

        private readonly IDialogService dialogService;

        public ObservableCollection<string> Files { get; set; }

        public ObservableCollection<Location> Locations { get; set; }

        public Location SelectedLocation { get; set; }

        public string SelectedSeason { get; set; }

        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                RaisePropertyChanged("ProgressValue");
            }
        }

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
            var locationSection = (NameValueCollection)ConfigurationManager.GetSection("locations");

            Locations = new ObservableCollection<Location>();

            foreach (var location in locationSection.AllKeys)
            {
                Locations.Add(new Location() { Id = location, Name = locationSection[location] });
            }

            this.dialogService = dialogService;

            DropCommand = new RelayCommand<DragEventArgs>(e =>
            {
                SelectedSeason = ((System.Windows.Shapes.Path)e.Source).Tag.ToString();

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    Files = new ObservableCollection<string>((string[])e.Data.GetData(DataFormats.FileDrop));
                }

                LocationDialogVisible = true;
            });

            ExitCommand = new RelayCommand(() =>
            {
                System.Windows.Application.Current.Shutdown();
            });

            UploadClickCommand = new RelayCommand(() =>
            {
                LocationDialogVisible = false;

                CopyFilesToLocalCache(this.Files.ToArray<string>());
            });
        }

        private async void CopyFilesToLocalCache(string[] files)
        {
            string cacheLocation = System.IO.Path.Combine(ConfigurationManager.AppSettings["localCache"].ToString() ?? AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString());

            var fileCopyManifest = new Dictionary<string, string>();

            double size = 0;

            var manifest = new List<string>();

            foreach (string filename in files)
            {
                var fileAttributes = File.GetAttributes(filename);

                if (fileAttributes.HasFlag(FileAttributes.Directory))
                {
                    string[] filePaths = Directory.GetFiles(filename, "*.*", SearchOption.AllDirectories);

                    manifest.AddRange(filePaths);
                }
                else
                {
                    manifest.Add(filename);
                }
            }

            string lastModifiedDateTime = string.Empty;

            foreach (string filename in manifest)
            {
                if (lastModifiedDateTime == string.Empty)
                {
                    var lastWriteTime = System.IO.File.GetLastWriteTime(filename);
                    lastModifiedDateTime = lastWriteTime.ToString("yyyy'-'MM'-'dd'-'HH") + "00";
                }

                string destination = Path.Combine(cacheLocation, lastModifiedDateTime);

                Directory.CreateDirectory(destination);

                string destinationFile = Path.Combine(destination, Path.GetFileName(filename));

                fileCopyManifest.Add(filename, destinationFile);
                var fileInfo = new FileInfo(filename);
                size += fileInfo.Length;
            }

            ProgressValueMax = size;

            await AsyncFileCopy.CopyFiles(fileCopyManifest, progress => ProgressValue = progress);

            string[] subFolders = Directory.GetDirectories(cacheLocation);

            foreach (string folder in subFolders)
            {
                RunAzCopy(folder, cacheLocation);
            }
        }

        private void RunAzCopy(string sourceDirectory, string parentDirectory)
        {
            var command = new StringBuilder("/C azcopy cp \"[sourceDirectory]\" \"[sasUri]\" --recursive=true --put-md5 & rmdir /q/s \"[parentDirectory]\"");

            string account = ConfigurationManager.AppSettings["storageAccountName"];
            string containerName = this.SelectedLocation.Id + "-" + this.SelectedSeason;
            string key = ConfigurationManager.AppSettings["key"];

            var cloudStorageAccount = new CloudStorageAccount(new Microsoft.Azure.Storage.Auth.StorageCredentials(account, key), true);

            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);

            string sasUri = GetContainerSasUri(cloudBlobContainer);

            command.Replace("[sourceDirectory]", sourceDirectory);
            command.Replace("[sasUri]", sasUri);
            command.Replace("[parentDirectory]", parentDirectory);

            string location = AppDomain.CurrentDomain.BaseDirectory + "AzCopy";

            using (var process = new Process())
            {
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;

                process.StartInfo.FileName = "cmd.exe";

                process.StartInfo.WorkingDirectory = location;

                process.StartInfo.Arguments = command.ToString();

                process.Start();
            }
        }

        private static string GetContainerSasUri(CloudBlobContainer container, string storedPolicyName = null)
        {
            string sasContainerToken;

            // If no stored policy is specified, create a new access policy and define its constraints.
            if (storedPolicyName == null)
            {
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
                // to construct a shared access policy that is saved to the container's shared access policies.
                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessStartTime = DateTime.UtcNow,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                    Permissions = SharedAccessBlobPermissions.Write //| SharedAccessBlobPermissions.List
                };

                // Generate the shared access signature on the container, setting the constraints directly on the signature.
                sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            }
            else
            {
                // Generate the shared access signature on the container. In this case, all of the constraints for the
                // shared access signature are specified on the stored access policy, which is provided by name.
                // It is also possible to specify some constraints on an ad hoc SAS and others on the stored access policy.
                sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);
            }

            // Return the URI string for the container, including the SAS token.
            return container.Uri + sasContainerToken;
        }
    }
}