using Firebase.Auth;
namespace TruckSlip.ViewModels
{
    public partial class ResetPasswordViewModel: BaseViewModel
    {
        [ObservableProperty] private string email = string.Empty;
        private readonly FirebaseAuthClient _firebaseAuthClient;

        public ResetPasswordViewModel(FirebaseAuthClient firebaseAuthClient) 
        {
            _firebaseAuthClient = firebaseAuthClient; 
        }

        [RelayCommand]
        public async Task ResetPassword()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Email))
                    throw new ArgumentException("Please enter your email address.");
                await _firebaseAuthClient.ResetEmailPasswordAsync(Email);
                await ShowNotification("Password reset email sent. Please check your inbox.");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
