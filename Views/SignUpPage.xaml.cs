using Firebase.Auth;

namespace TruckSlip.Views;

public partial class SignUpPage
{
    public SignUpPage(SignUpViewModel signUpViewModel)
    {
        InitializeComponent();
        BindingContext = signUpViewModel;
    }
}