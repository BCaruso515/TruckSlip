namespace TruckSlip.ViewModels
{
    public partial class OrderViewModel : BaseViewModel
    {
        [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
        [ObservableProperty] private Jobsite _selectedJobsite = new();
        [ObservableProperty] private Company _selectedCompany = new();
        [ObservableProperty] private Order _selectedOrder = new();
        [ObservableProperty] private bool _isDelivery;
        [ObservableProperty] private bool _isPickup;
        [ObservableProperty] private bool _isComboboxEnabled;

        private static bool SkipRefresh { get; set; } = false;

        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;
        private int _index = -1;

        public OrderViewModel(IDataServiceProvider provider)
        {
            _provider = provider;
            RefreshButtonState();
        }

        [RelayCommand]
        public async Task Appearing()
        {
            IsComboboxEnabled = true;
            SetButtonText(!IsComboboxEnabled);

            if (SkipRefresh)
            { 
                SkipRefresh = false;
                return; 
            }

            EnableAdd = await RefreshCompanyAsync(Database);
            if (!EnableAdd)
            {
                //Alert to add Company information
                await Shell.Current.DisplayAlert("No Companies Found",
                    "You must add a company before you can add an order.", "Ok");
                EnableDelete = EnableEdit = false;
                return;
            }
            SelectedCompany = Companies.First(); 
        }

        [RelayCommand]
        public void CheckboxChanged(string sender)
        {
            if (sender == nameof(IsDelivery))
            {
                IsPickup = !IsDelivery;
                return;
            }
            
            if (sender == nameof(IsPickup))
                    IsDelivery = !IsPickup;
        }

        [RelayCommand]
        public void SelectedOrderChanged()
        {
            IsDelivery = Convert.ToBoolean(SelectedOrder.OrderTypeId);
            IsPickup = !IsDelivery;

            if (!string.IsNullOrEmpty(SelectedOrder.Date))
                SelectedDate = Convert.ToDateTime(SelectedOrder.Date);
        }

        [RelayCommand]
        public async Task SelectedCompanyChanged()
        {
            EnableAdd = await RefreshJobsiteAsync(Database, SelectedCompany.CompanyId);
            if (!EnableAdd)
            {
                EnableDelete = EnableEdit = false;
                SelectedJobsite = new();
                return;
            }

            SelectedJobsite = Jobsites.First();
            EnableDelete = EnableEdit = true;
        }

        [RelayCommand]
        public async Task SelectedJobsiteChanged()
        {
            if (!await RefreshOrdersAsync(Database, SelectedJobsite.JobsiteId))
            {
                EnableDelete = EnableEdit = false;
                SelectedOrder = new();
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
                    IsComboboxEnabled = false;
                    SetButtonText(!IsComboboxEnabled);
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
                        IsComboboxEnabled = true;
                        SetButtonText(!IsComboboxEnabled);
                        EnableDelete = EnableEdit = await RefreshOrdersAsync(Database, SelectedJobsite.JobsiteId);
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
                    IsComboboxEnabled = false;
                    SetButtonText(!IsComboboxEnabled);
                    _index = Orders.IndexOf(SelectedOrder);
                    SelectedOrder = new();
                    EnableEdit = true;
                    view.Focus();
                }
                else
                {
                    IsComboboxEnabled = true;
                    SetButtonText(!IsComboboxEnabled);
                    EnableDelete = EnableEdit = await RefreshOrdersAsync(Database, SelectedJobsite.JobsiteId);

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
                EnableDelete = EnableEdit = await RefreshOrdersAsync(Database, SelectedJobsite.JobsiteId);

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
            SkipRefresh = true;
            await Shell.Current.GoToAsync(nameof(OrderItemsPage), true,
                new Dictionary<string, object>
                {
                    { "SelectedOrder", SelectedOrder }
                });
        }
    }
}
