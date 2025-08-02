namespace TruckSlip.Views;

public partial class UnitTypesPage : ContentPage
{
	public UnitTypesPage(UnitTypesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}