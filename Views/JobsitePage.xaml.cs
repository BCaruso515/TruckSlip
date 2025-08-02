namespace TruckSlip.Views;

public partial class JobsitePage : ContentPage
{
	public JobsitePage(JobsiteViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}