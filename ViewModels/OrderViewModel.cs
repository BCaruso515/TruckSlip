namespace TruckSlip.ViewModels
{
    public partial class OrderViewModel : BaseViewModel
    {
        [ObservableProperty] private Jobsite _selectedJobsite = new();
        [ObservableProperty] private DateTime _selectedDate = DateTime.Today;

        private Order _selectedOrder = new();
        private bool _isDelivery;
        private bool _isPickup;
        private int _index = -1;
        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (value == null) return;
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
                
                IsDelivery = Convert.ToBoolean(_selectedOrder.OrderTypeId);
                OnPropertyChanged(nameof(IsDelivery));

                if (! string.IsNullOrEmpty(_selectedOrder.Date))
                    SelectedDate = Convert.ToDateTime(_selectedOrder.Date);
                OnPropertyChanged(nameof(SelectedDate));
            }
        }

        public bool IsDelivery
        {
            get => _isDelivery;
            set
            { 
                if (value == _isDelivery) return;
                _isDelivery = value;
                IsPickup = !value;
                OnPropertyChanged(nameof(IsDelivery));
                OnPropertyChanged(nameof(IsPickup));
            }
        }

        public bool IsPickup
        {
            get => _isPickup;
            set
            {
                if (value == _isPickup) return;
                _isPickup = value;
                IsDelivery = !value;
                OnPropertyChanged(nameof(IsPickup));
                OnPropertyChanged(nameof(IsDelivery));
            }
        }

        public OrderViewModel(IDataServiceProvider provider)
        {
            _provider = provider;
            RefreshButtonState();
        }

        [RelayCommand]
        public async Task Appearing()
        {
            SetButtonText(false);

            EnableAdd = await RefreshJobsiteAsync(Database);
            if (!EnableAdd)
            {
                //Alert to add Jobsite information
                await Shell.Current.DisplayAlert("Alert!", "Jobsite must be added to continue...", "Ok");
                EnableDelete = EnableEdit = false;
                return;
            }
            SelectedJobsite = Jobsites.First();

            if (!await RefreshOrdersAsync(Database))
            {
                EnableDelete = EnableEdit = false;
                return;
            }
            SelectedOrder = Orders.First();
            EnableDelete = EnableEdit = true;
        }

        [RelayCommand]
        public async Task EditSave(View view)
        {
            try
            {
                if (EditSaveButtonText == "Edit")
                {
                    SetButtonText(true);
                    _index = Orders.IndexOf(SelectedOrder);
                    view.Focus();
                }
                else
                {
                    SelectedOrder.OrderTypeId = Convert.ToByte(IsDelivery);
                    SelectedOrder.JobsiteId = SelectedJobsite.JobsiteId;
                    SelectedOrder.Date = SelectedDate.ToString("d");
                    if (await Database.AddOrUpdateOrderAsync(SelectedOrder))
                    {
                        SetButtonText(false);
                        EnableDelete = EnableEdit = await RefreshOrdersAsync(Database);
                        SelectedOrder = Orders.Where(x => x.OrderId == SelectedOrder.OrderId).First();
                    }
                }
            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }
        }

        [RelayCommand]
        public async Task AddCancel(View view)
        {
            try
            {
                if (AddCancelButtonText == "Add")
                {
                    SetButtonText(true);
                    _index = Orders.IndexOf(SelectedOrder);
                    SelectedOrder = new();
                    EnableEdit = true;
                    view.Focus();
                }
                else
                {
                    SetButtonText(false);
                    EnableDelete = EnableEdit = await RefreshOrdersAsync(Database);

                    if (_index > -1)
                        SelectedOrder = Orders[_index];
                }
            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }
        }

        [RelayCommand]
        public async Task Delete()
        {
            try
            {
                _index = Orders.IndexOf(SelectedOrder);

                if (!await Shell.Current.DisplayAlert("WARNING!",
                    "Deleting this order also deletes all records related to this order, " +
                    "are you sure you want to proceed? This action can not be undone.",
                    "Yes", "No")) return;

                await Database.DeleteOrderAsync(SelectedOrder);
                EnableDelete = EnableEdit = await RefreshOrdersAsync(Database);

                if (Orders.Count == 0)
                    return;

                if (_index > Orders.IndexOf(Orders.Last()))
                    SelectedOrder = Orders.Last();
                else
                    SelectedOrder = Orders[_index];
            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }

            finally
            { _index = -1; }
        }

        [RelayCommand]
        public async Task AddItems()
        {
            await Shell.Current.GoToAsync(nameof(OrderItemsPage), true,
                new Dictionary<string, object>
                {
                    { "SelectedOrder", SelectedOrder }
                });
        }
    }
}
