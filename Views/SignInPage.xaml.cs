using Firebase.Auth;

namespace TruckSlip.Views;

public partial class SignInPage
{
    public SignInPage(SignInViewModel signInViewModel)
    {
        InitializeComponent();
        BindingContext = signInViewModel;
    }
}

    


