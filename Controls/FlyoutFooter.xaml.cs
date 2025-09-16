using Firebase.Auth;

namespace TruckSlip.Controls;

public partial class FlyoutFooter : VerticalStackLayout
{
    FirebaseAuthClient _firebaseAuthClient;

    public FlyoutFooter(FirebaseAuthClient firebaseAuthClient)
	{
		InitializeComponent();
        _firebaseAuthClient = firebaseAuthClient;
    }

    private async void Button_Clicked(object sender, EventArgs e)
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
}