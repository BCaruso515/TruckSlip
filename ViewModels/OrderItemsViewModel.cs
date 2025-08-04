namespace TruckSlip.ViewModels
{
    [QueryProperty(nameof(SelectedOrder), "SelectedOrder")]
    public partial class OrderItemsViewModel : BaseViewModel
    {
        [ObservableProperty] private Product _selectedProduct = new();
        [ObservableProperty] private OrderItem _selectedOrderItem = new();
        [ObservableProperty] private UnitType _selectedUnitType = new();
        [ObservableProperty] private Order _selectedOrder = new();
        [ObservableProperty] bool _enableAddToOrder;

        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;

        public OrderItemsViewModel(IDataServiceProvider provider)
        {
            _provider = provider;
            RefreshButtonState();
        }

        [RelayCommand]
        public async Task Appearing()
        {
            if (SelectedOrder == null || SelectedOrder.OrderId == 0)
            {
                await Shell.Current.DisplayAlert("Error!", "Order cannot be null!", "Ok");
                return;
            }

            if (!await RefreshUnitTypesAsync(Database))
            {
                //Alert to add unit types
                await Shell.Current.DisplayAlert("Alert!", "Unit Types must be added to continue...", "Ok");
                return;
            }

            if (!await RefreshProductsAsync(Database))
            {
                EnableAddToOrder = false;
                return;
            }
            SelectedProduct = Products.First();
            EnableAddToOrder = true;

            await RefreshItemsQueryAsync(Database, SelectedOrder.OrderId);
        }

        [RelayCommand]
        public void SelectedProductChanged()
        {
            if (SelectedProduct == null || SelectedProduct.ProductId == 0) return;

            SelectedUnitType = UnitTypes.First(UnitTypes => UnitTypes.UnitId == SelectedProduct.UnitId);
            SelectedOrderItem.Quantity = 1;
            OnPropertyChanged(nameof(SelectedOrderItem));
        }

        [RelayCommand]
        public async Task AddToOrder()
        {
            if (Orders.Count == 0) return;
            if (Products.Count == 0) return;

            await Database.AddOrUpdateOrderItemAsync(new()
            {
                OrderId = SelectedOrder.OrderId,
                ProductId = SelectedProduct.ProductId,
                Quantity = SelectedOrderItem.Quantity
            });
            await RefreshItemsQueryAsync(Database, SelectedOrder.OrderId);
        }

        [RelayCommand]
        public async Task RemoveFromOrder(OrderItemsQuery itemsQuery)
        {
            if (itemsQuery == null) return;
            ObservableCollection<OrderItem> results = await Database.GetOrderItemAsync();
            var result = results.Where(x => x.OrderItemId == itemsQuery.OrderItemId).First();
            await Database.DeleteOrderItemAsync(result);
            await RefreshItemsQueryAsync(Database, SelectedOrder.OrderId);
        }
        [RelayCommand]
        public async Task ExportOrder()
        {
            var jobsites = await Database.GetJobsiteAsync();
            var jobsite = jobsites.Where(x => x.JobsiteId == SelectedOrder.JobsiteId).First();

            if (! await RefreshCompanyAsync(Database))
                throw new Exception("Company cannot be null!");
            var company = Companies.Where(x => x.CompanyId == jobsite.CompanyId).First(); 

            var orderReport = new OrderReport(company, jobsite, SelectedOrder, ItemsQuery);
            orderReport.GeneratePdfAndShow();
        }
    }
}
