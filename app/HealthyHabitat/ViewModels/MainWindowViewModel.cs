﻿namespace HealthyHabitat.ViewModels
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Blob;
    using MvvmDialogs;
    using SixSeasons.Helpers;
    using SixSeasons.Models;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using Microsoft.Identity.Client;

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

        public RelayCommand SignInCommand
        {
            get;
            private set;
        }

        public RelayCommand SignOutCommand
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

        private bool _signInButtonVisible;
        public bool SignInButtonVisible
        {
            get { return _signInButtonVisible; }
            set
            {
                _signInButtonVisible = value;
                RaisePropertyChanged("SignInButtonVisible");
            }
        }

        private bool _signOutButtonVisible;
        public bool SignOutButtonVisible
        {
            get { return _signOutButtonVisible; }
            set
            {
                _signOutButtonVisible = value;
                RaisePropertyChanged("SignOutButtonVisible");
            }
        }

        private string _signedInUserName;
        public string SignedInUserName
        {
            get { return _signedInUserName; }
            set
            {
                _signedInUserName = value;
                RaisePropertyChanged("SignedInUserName");
            }
        }

        private string _displayMessage;
        public string DisplayMessage
        {
            get { return _displayMessage; }
            set
            {
                _displayMessage = value;
                RaisePropertyChanged("DisplayMessage");
            }
        }

        // Auth definitions ------------------------------------------------------------------- 
        private readonly string ClientId = "18dd432c-1e9e-4da7-93f6-f7d60e3ea712"; // ToDo: ConfigurationManager.AppSettings["ClientID"].ToString()
        private readonly string ClientScopes = "user.read https://storage.azure.com/user_impersonation"; // ToDo: ConfigurationManager.AppSettings["ClientScopes"].ToString()

        private IPublicClientApplication PublicClientApp { get; set; }
        private AuthenticationResult AuthResult { get; set; }

        // End Auth definitions -------------------------------------------------------------------

        public MainWindowViewModel(IDialogService dialogService)
        {
            // Auth init -------------------------------------------------------------------
            SignInButtonVisible = true;
            DisplayMessage = "Please sign in to continue";
            SignedInUserName = "";
            PublicClientApp = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
                .WithLogging((level, message, containsPii) =>
                {
                    Debug.WriteLine($"MSAL: {level} {message} ");
                }, Microsoft.Identity.Client.LogLevel.Warning, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                .Build();

            // End Auth init -------------------------------------------------------------------

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

            SignInCommand = new RelayCommand(async () =>
            {
                //await Authorize();
                //DisplayBasicTokenInfo();
            });

            SignOutCommand = new RelayCommand(async () =>
            {
                //await DeAuthorize();
                //DisplayBasicTokenInfo();
            });
        }

        private async void CopyFilesToLocalCache(string[] files, string location, string season)
        {

            string cacheLocation = System.IO.Path.Combine(ConfigurationManager.AppSettings["localCache"].ToString() ?? AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString());

            Dictionary<string, string> fileCopyManifest = new Dictionary<string, string>();

            double size = 0;

            List<string> manifest = new List<string>();

            // Test for folders and flatten into files
            foreach(string filename in files)
            {
                FileAttributes attr = File.GetAttributes(filename);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    string[] filePaths = Directory.GetFiles(filename, "*.*", SearchOption.AllDirectories);

                    manifest.AddRange(filePaths);
                }
                else
                {
                    manifest.Add(filename);
                }
            }

            foreach (string filename in manifest)
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

            string[] subFolders = Directory.GetDirectories(cacheLocation);

            foreach (string folder in subFolders)
            {
                await InitiateAzCopy(folder, cacheLocation);
            }
            
        }

        private async Task InitiateAzCopy(string sourceDir, string parentDir)
        {
            // cmd command to run AzCopy and remove local cache folder
            StringBuilder command = new StringBuilder("/C azcopy cp \"[sourceDir]\" \"[sasUri]\" --recursive=true --put-md5 & rmdir /q/s \"[parentDir]\"");

            string account = ConfigurationManager.AppSettings["storageAccountName"];
            string containerName = ConfigurationManager.AppSettings["containerName"];
            string key = ConfigurationManager.AppSettings["key"];

            CloudStorageAccount storageAccount = new CloudStorageAccount(
                new Microsoft.Azure.Storage.Auth.StorageCredentials(account, key), true);

            // Create a blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get a reference to a container named "mycontainer."
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            string sasUri = GetContainerSasUri(container);

            command.Replace("[sourceDir]", sourceDir);
            command.Replace("[sasUri]", sasUri);
            command.Replace("[parentDir]", parentDir);

            string location = AppDomain.CurrentDomain.BaseDirectory + "AzCopy";

            using (Process cmd = new Process())
            {
                cmd.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;

                cmd.StartInfo.FileName = "cmd.exe";

                cmd.StartInfo.WorkingDirectory = location;
                //cmd.StartInfo.CreateNoWindow = true;
                //cmd.StartInfo.UseShellExecute = false;
                //cmd.StartInfo.RedirectStandardInput = true;
                //cmd.StartInfo.RedirectStandardOutput = true;

                cmd.StartInfo.Arguments = command.ToString(); ;

                cmd.Start();
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
