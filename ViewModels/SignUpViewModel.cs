using Firebase.Auth;
using FirebaseAdmin.Auth;

namespace TruckSlip.ViewModels
{
    public partial class SignUpViewModel : BaseViewModel
    {
        private readonly FirebaseAuthClient _firebaseAuthClient;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string confirmPassword = string.Empty;
        [ObservableProperty] private string displayName = string.Empty;

        public SignUpViewModel(FirebaseAuthClient firebaseAuthClient)
        {
            _firebaseAuthClient = firebaseAuthClient;
        }

        [RelayCommand]
        public async Task SignUp()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(DisplayName))
            {
                await Shell.Current.DisplayAlert("Error", "Please fill in all fields", "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                await Shell.Current.DisplayAlert("Error", "Passwords do not match", "OK");
                return;
            }

            await CreateUser(Email, Password, DisplayName);
        }

        private async Task CreateUser(string email, string password, string displayName)
        {
            try
            {
                var userCredential = await _firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(email, password, displayName)
                    ?? throw new Exception("User creation failed");

                Dictionary<string, object> claims = new()
                {
                    { "IsUser", true }
                };
                
                await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(userCredential.User.Uid, claims );
                await ShowNotification("User created successfully");
                await Shell.Current.GoToAsync("..");                
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
