using Firebase.Auth;
using Firebase.Database;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Configuration;

namespace TruckSlip.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly FirebaseAuthClient _firebaseAuthClient;
        private readonly IDataServiceProvider _provider;
        private readonly IConfiguration _configuration;
        private readonly string _sqliteDbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "TruckSlip.db3"); // Local SQLite database path
        private IDataService Database => _provider.Current; 
        private string DatabaseUrl => _configuration.GetSection("Firebase")["DatabaseUrl"] ?? throw new Exception("Database Error");
        private bool _isDatabaseConnected = false;
        [ObservableProperty] private bool isAdmin;
        [ObservableProperty] private bool isUser;
        [ObservableProperty] private bool isRemote;

        public bool UseRemote
        {
            get => Preferences.Get("UseRemote", false);
            set
            {
                Preferences.Set("UseRemote", value);
                _isDatabaseConnected = false;
                OnPropertyChanged(nameof(UseRemote));
            }
        }

        public HomeViewModel(IDataServiceProvider provider, 
            FirebaseAuthClient firebaseAuthClient,
            IConfiguration configuration)
        {
            _provider = provider;
            _firebaseAuthClient = firebaseAuthClient;
            _configuration = configuration;
        }

        [RelayCommand]
        public async Task Appearing()
        {
            if (_isDatabaseConnected) return;
            
            try
            {
                Shell.Current.FlyoutHeader = new Controls.FlyoutHeader(_firebaseAuthClient);
                await GetRolesAsync();
                if (!IsAdmin)
                    UseRemote = IsRemote;

                if (UseRemote)
                {
                    _provider.UseRemote();
                    var user = _firebaseAuthClient.User;
                    if (user == null) throw new ArgumentNullException(nameof(user), "User cannot be null.");
                    RefreshDataConnection(user);
                }
                else
                {
                    _provider.UseLocal();
                }

                if (Database == null) throw new Exception("Database Error");

                await Database.InitializeDatabaseAsync();

                _isDatabaseConnected = true;
            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async Task GetRolesAsync()
        {
            IsAdmin = await IsInRole("IsAdmin");
            IsUser = await IsInRole("IsUser");
            IsRemote = await IsInRole("IsRemote");
        }

        private async Task<bool> IsInRole(string role)
        {
            var user = _firebaseAuthClient.User;
            if (user == null) return false;
            var currentUser = await FirebaseAuth.DefaultInstance.GetUserAsync(user.Uid);
            return currentUser.CustomClaims != null &&
                   currentUser.CustomClaims.ContainsKey(role) &&
                   (bool)currentUser.CustomClaims[role];
        }

        private void RefreshDataConnection(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "User cannot be null.");

            var firebaseClient = new FirebaseClient(DatabaseUrl, new FirebaseOptions
            {
                AuthTokenAsyncFactory = async () => await user.GetIdTokenAsync(true)
            });
            var newService = new FirebaseDataService(firebaseClient);
            _provider.UpdateRemote(newService);
        }

        [RelayCommand]
        public async Task SignOut()
        {
            try
            {
                _firebaseAuthClient.SignOut();
                Preferences.Set("RememberMe", false);
                await Shell.Current.GoToAsync($"//{nameof(SignInPage)}");
            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error", ex.Message, "OK"); }
        }

        [RelayCommand]
        public static async Task NavigateToPage(string pageName)
        => await Shell.Current.GoToAsync(pageName);        

        [RelayCommand]
        public async Task ImportData()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select data file *.db" });
                if (result == null) return;

                if (UseRemote)
                {
                    File.Copy(result.FullPath, _sqliteDbPath, true);
                    await Database.InitializeDatabaseAsync();
                    SqliteToFirebaseHelper helper = new(_provider);
                    await helper.SyncAllAsync();
                }
                else
                {
                    await Database.CloseDatabaseAsync();
                    File.Copy(result.FullPath, _sqliteDbPath, true);
                }

                await ShowNotification("Database has been restored from backup");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }
        }

        [RelayCommand]
        public async Task BackupData()
        {

            if (UseRemote)
            {
                FirebaseToSqliteHelper helper = new(_provider);
                await helper.CreateBackupAsync();
            }
            else
                await Database.CloseDatabaseAsync();

#if !WINDOWS
            await BackupDatabaseIOS();
#else
            await BackupDatabaseWindows(_sqliteDbPath, CancellationToken.None);
#endif
        }

        private async Task BackupDatabaseIOS()
            => await Share.Default.RequestAsync(new ShareFileRequest("Backup Data", new ShareFile(_sqliteDbPath)));

        private async Task BackupDatabaseWindows(string dataLocation, CancellationToken cancellationToken)
        {
#if WINDOWS
            var result = await FolderPicker.PickAsync(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), cancellationToken);
            if (!result.IsSuccessful) return;

            string newFileName = await GetFileName(Path.GetFileNameWithoutExtension(dataLocation));
            File.Copy(dataLocation, Path.Combine(result.Folder.Path, $"{newFileName}.db3"), true);
            
            await Shell.Current.DisplayAlert("Backup Successful",
                $"Data backup successful to {Path.Combine(result.Folder.Path, $"{newFileName}.db3")}", "OK");
#else
            await Task.CompletedTask;
#endif
        }

        [RelayCommand]
        public async Task DeleteAllData()
        {
            try
            {
                if (!await Shell.Current.DisplayAlert("WARNING!",
                    "This will delete all application data. " +
                    "This includes all orders, unit types, jobsites, companies, and inventory. " +
                    "Are you sure you want to proceed? This action can not be undone.",
                    "Yes", "No")) return;

                await Database.DropTablesAsync();
                await Database.CreateTablesAsync();
                await ShowNotification("Database has been reset to clean install");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }
        }

        [RelayCommand]
        public async Task LoadDefaultData()
        {
            try
            {
                if (!await Shell.Current.DisplayAlert("WARNING!",
                    "This will delete all current application data and replace it with defaults. " +
                    "This includes all orders, unit types, jobsites, companies, and inventory. " +
                    "Are you sure you want to proceed? This action can not be undone.",
                    "Yes", "No")) return;

                var helper = new DefaultDataHelper(_provider);
                await helper.LoadDefaultsAsync();
                await ShowNotification("Default data has been loaded");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }
        }

        private static async Task<string> GetFileName(string initialValue)
        {
            string? fileName = await Shell.Current.DisplayPromptAsync
            (
                title: "Edit File Name",
                message: "File Name",
                accept: "OK",
                cancel: "Cancel",
                placeholder: initialValue,
                maxLength: 255,
                keyboard: Keyboard.Text,
                initialValue: initialValue
            );
            return fileName;
        }
    }
}
