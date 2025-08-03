namespace TruckSlip.ViewModels
{
    public partial class OrderViewModel : BaseViewModel
    {
        [ObservableProperty] private DateTime _selectedDate = DateTime.Today;

        private Jobsite _selectedJobsite = new();
        private Company _selectedCompany = new();
        private Order _selectedOrder = new();
        private bool _isDelivery;
        private bool _isPickup;
        private int _index = -1;
        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;

        public Company SelectedCompany
        {
            get => _selectedCompany;

            set
            {
                if (_selectedCompany == value) return;
                _selectedCompany = value;
                Task.Run(async () => await SelectedCompanyChanged());
                OnPropertyChanged();
            }
        }

        public Jobsite SelectedJobsite
        {
            get => _selectedJobsite;
            set
            {
                if (_selectedJobsite == value) return;
                _selectedJobsite = value;
                Task.Run(async () => await SelectedJobsiteChanged());
                OnPropertyChanged();
            }
        }

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
            await SelectedCompanyChanged();            

            if (!await RefreshOrdersAsync(Database, SelectedJobsite.JobsiteId))
            {
                EnableDelete = EnableEdit = false;
                return;
            }
            SelectedOrder = Orders.First();
            EnableDelete = EnableEdit = true;
        }

        [RelayCommand]
        public async Task SelectedCompanyChanged()
        {
            if (!await RefreshJobsiteAsync(Database, SelectedCompany.CompanyId))
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
                    SetButtonText(true);
                    _index = Orders.IndexOf(SelectedOrder);
                    SelectedOrder = new();
                    EnableEdit = true;
                    view.Focus();
                }
                else
                {
                    SetButtonText(false);
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
            await Shell.Current.GoToAsync(nameof(OrderItemsPage), true,
                new Dictionary<string, object>
                {
                    { "SelectedOrder", SelectedOrder }
                });
        }
    }
}
