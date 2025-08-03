namespace TruckSlip.Views;

public partial class CompanyPage : ContentPage
{
	public CompanyPage(CompanyViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}