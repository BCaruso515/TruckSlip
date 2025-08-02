namespace TruckSlip.Views;

public partial class OrderItemsPage : ContentPage
{
	public OrderItemsPage(OrderItemsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}