using Firebase.Auth;
using FirebaseAdmin.Auth;

namespace TruckSlip.Controls;

public partial class FlyoutHeader : VerticalStackLayout
{
	private readonly FirebaseAuthClient _firebaseAuthClient;

    public FlyoutHeader(FirebaseAuthClient firebaseAuthClient)
	{
		InitializeComponent();
		_firebaseAuthClient = firebaseAuthClient;
        SetLabels();
    }

	private async void SetLabels()
	{
		if (_firebaseAuthClient.User == null) return;
        var userInfo = await FirebaseAuth.DefaultInstance.GetUserAsync(_firebaseAuthClient.User.Uid);

        userEmailLabel.Text = userInfo.Email;
        userNameLabel.Text = userInfo.DisplayName;
        userRolesLabel.Text = userInfo.CustomClaims != null ? string.Join(", ", userInfo.CustomClaims.Keys) : "";
    }
}