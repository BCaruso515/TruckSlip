namespace TruckSlip.Views;

public partial class ProductPage : ContentPage
{
	public ProductPage(ProductViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}