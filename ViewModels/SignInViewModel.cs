using Firebase.Auth;

namespace TruckSlip.ViewModels
{
    
        public partial class SignInViewModel : BaseViewModel
        {
            private readonly FirebaseAuthClient _firebaseAuthClient;
            [ObservableProperty] private string email = string.Empty;
            [ObservableProperty] private string password = string.Empty;
            [ObservableProperty] private bool rememberMe;

            public SignInViewModel(FirebaseAuthClient firebaseAuthClient)
            {
                _firebaseAuthClient = firebaseAuthClient;
                RememberMe = Preferences.Get("RememberMe", false);
            }

            [RelayCommand]
            public async Task SignUpPage() =>
                await Shell.Current.GoToAsync(nameof(SignUpPage), true);

            [RelayCommand]
            public async Task SignIn()
            {
                try
                {
                    ValidateInputFields(Email, Password);
                    await SignInUser(Email, Password);
                    Preferences.Set("RememberMe", RememberMe);
                    await Shell.Current.GoToAsync($"//{nameof(HomePage)}", true);
                }
                catch (Exception ex)
                { await Shell.Current.DisplayAlert("Error", ex.Message, "OK"); }
            }

            [RelayCommand]
            public async Task Appearing()
            {
                var savedUser = Preferences.Get("RememberMe", false);
                if (!savedUser) return;
                if (_firebaseAuthClient.User != null)
                {
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"//{nameof(HomePage)}", true);
                    return;
                }

                _firebaseAuthClient.SignOut();
            }

            private async Task<UserCredential> SignInUser(string email, string password)
                => await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(email, password);

            private static void ValidateInputFields(string email, string password)
            {
                if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));

                if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be empty.", nameof(password));
            }
        }
    }
