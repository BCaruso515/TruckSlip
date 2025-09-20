using TruckSlip.Report;

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
        private bool _isPageInitialized = false;
        private IDataService Database => _provider.Current;

        public OrderItemsViewModel(IDataServiceProvider provider)
        {
            _provider = provider;
            RefreshButtonState();
        }

        [RelayCommand]
        public async Task Appearing()
        {
            if (_isPageInitialized) return;
            if (SelectedOrder == null || SelectedOrder.OrderId == 0)
            {
                await Shell.Current.DisplayAlert("Error!", "Order cannot be null!", "Ok");
                return;
            }

            if (!await RefreshOrdersAsync(Database, SelectedOrder.JobsiteId))
            {
                await Shell.Current.DisplayAlert("Error!", "There was an error accessing your order, Please try again.", "Ok");
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
            SelectedProduct = Products.FirstOrDefault() ?? new();
            EnableAddToOrder = true;

            await RefreshItemsQueryAsync(Database, SelectedOrder.OrderId);
            _isPageInitialized = true;
        }

        [RelayCommand]
        public void SelectedProductChanged()
        {
            if (SelectedProduct == null || SelectedProduct.ProductId == 0) return;

            SelectedUnitType = UnitTypes.FirstOrDefault(unit => unit.UnitId == SelectedProduct.UnitId) ?? new();
            SelectedOrderItem.Quantity = 1;
            OnPropertyChanged(nameof(SelectedOrderItem));
            OnPropertyChanged(nameof(SelectedUnitType));
        }

        [RelayCommand]
        public async Task AddToOrder()
        {
            if (Orders.Count == 0 || Products.Count == 0) return;

            await Database.AddOrUpdateOrderItemAsync(new()
            {
                OrderId = SelectedOrder.OrderId,
                ProductId = SelectedProduct.ProductId,
                Quantity = SelectedOrderItem.Quantity
            });
            if (!await RefreshItemsQueryAsync(Database, SelectedOrder.OrderId)) return;
            await ShowNotification("Item Added");
        }

        [RelayCommand]
        public async Task RemoveFromOrder(OrderItemsQuery itemsQuery)
        {
            if (itemsQuery == null) return;
            ObservableCollection<OrderItem> results = await Database.GetOrderItemAsync();
            var result = results.FirstOrDefault(x => x.OrderItemId == itemsQuery.OrderItemId);
            if (result == null)
            {
                await ShowNotification("Order item not found.");
                return;
            }
            await Database.DeleteOrderItemAsync(result);
            await RefreshItemsQueryAsync(Database, SelectedOrder.OrderId);
        }
        [RelayCommand]
        public async Task ExportOrder()
        {
            try
            {
                var jobsites = await Database.GetJobsiteAsync();
                var jobsite = jobsites.FirstOrDefault(x => x.JobsiteId == SelectedOrder.JobsiteId) 
                    ?? throw new Exception("Jobsite not found for this order.");
                if (!await RefreshCompanyAsync(Database) || Companies.Count == 0)
                    throw new Exception("Company cannot be null!");
                var company = Companies.FirstOrDefault(x => x.CompanyId == jobsite.CompanyId) 
                    ?? throw new Exception("Company not found for this jobsite.");
                var orderReport = new OrderReportService(company, jobsite, SelectedOrder, ItemsQuery);
                orderReport.GeneratePdfAndShow();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
