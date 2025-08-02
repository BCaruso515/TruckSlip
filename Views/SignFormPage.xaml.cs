namespace TruckSlip.Views;

public partial class SignFormPage : ContentPage
{
	public SignFormPage(SignFormViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}