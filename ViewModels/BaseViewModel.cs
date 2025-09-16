namespace TruckSlip.ViewModels
{
    public partial class BaseViewModel:ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Order> orders = [];
        [ObservableProperty] private ObservableCollection<Product> products = [];
        [ObservableProperty] private ObservableCollection<UnitType> unitTypes = [];
        [ObservableProperty] private ObservableCollection<InventoryType> inventoryTypes = [];
        [ObservableProperty] private ObservableCollection<Jobsite> jobsites = [];
        [ObservableProperty] private ObservableCollection<Company> companies = new();
        [ObservableProperty] private ObservableCollection<OrderItemsQuery> itemsQuery = [];


        [ObservableProperty] private string fontFamily = "free-solid-900";
        [ObservableProperty] private string editSaveButtonText = string.Empty;
        [ObservableProperty] private string addCancelButtonText = string.Empty;
        [ObservableProperty] private string deleteImage = FontAwesomeHelper.TrashCan;
        [ObservableProperty] private string editSaveImage = string.Empty;
        [ObservableProperty] private string addCancelImage = string.Empty;
        [ObservableProperty] private bool isEnabled;
        [ObservableProperty] private bool enableDelete;
        [ObservableProperty] private bool enableAdd;
        [ObservableProperty] private bool enableEdit;

        protected bool RefreshInventoryTypes()
        {
            InventoryTypes =
            [
                new() { TypeId = 1, TypeName = "Consumables" },
                new() { TypeId = 2, TypeName = "Equipment" }
            ];
            InventoryTypes = [.. InventoryTypes.OrderBy(x => x.TypeName)];
            return InventoryTypes.Count != 0;
        }

        protected async Task<bool> RefreshCompanyAsync(IDataService database)
        {
            Companies = await database.GetCompanyAsync();
            Companies = [.. Companies.OrderBy(x => x.Name)];
            return Companies.Count > 0;
        }

        protected async Task<bool> RefreshJobsiteAsync(IDataService database, int id)
        {
            Jobsites = await database.GetJobsiteAsync();
            Jobsites = [.. Jobsites.OrderBy(x => x.Name).Where(x => x.CompanyId == id)];
            return Jobsites.Count > 0;
        }

        protected async Task<bool> RefreshProductsAsync(IDataService database)
        {
            Products = await database.GetProductAsync();
            Products = [.. Products.OrderBy(x => x.Name)];
            return Products.Count != 0;
        }

        protected async Task<bool> RefreshUnitTypesAsync(IDataService database)
        {
            UnitTypes = await database.GetUnitTypesAsync();
            UnitTypes = [.. UnitTypes.OrderBy(x => x.UnitName)];
            return UnitTypes.Count != 0;
        }

        protected async Task<bool> RefreshOrdersAsync(IDataService database, int id)
        {
            Orders = await database.GetOrderAsync();
            Orders = [.. Orders.Where(x=> x.JobsiteId == id).OrderByDescending(x => Convert.ToDateTime(x.Date))];
            return Orders.Count != 0;
        }

        protected async Task<bool> RefreshItemsQueryAsync(IDataService database, int id)
        {
            ItemsQuery = await database.GetOrderItemsQueryAsync();
            ItemsQuery = [.. ItemsQuery.OrderBy(x=> x.Name).Where(x=> x.OrderId == id)];
            return ItemsQuery.Count != 0;
        }

        protected void SetButtonText(bool editing)
        {
            IsEnabled = editing;
            EnableDelete = !editing;
            if (editing)
            {
                EditSaveButtonText = "Save";
                EditSaveImage = FontAwesomeHelper.FloppyDisk;
                AddCancelButtonText = "Cancel";
                AddCancelImage = FontAwesomeHelper.Xmark;
            }
            else
            {
                EditSaveButtonText = "Edit";
                EditSaveImage = FontAwesomeHelper.Pencil;
                AddCancelButtonText = "Add";
                AddCancelImage = FontAwesomeHelper.Plus;
            }
        }

        protected void RefreshButtonState()
        {
            EditSaveButtonText = AddCancelButtonText = EditSaveImage = AddCancelImage = string.Empty;
            IsEnabled = EnableDelete = EnableAdd = EnableEdit = false;
        }

        public static async Task ShowNotification(string message)
        {
#if ANDROID || IOS
            await Toast.Make(message, ToastDuration.Short).Show();
#elif WINDOWS
            await Shell.Current.DisplayAlert("", message, "OK");
#else
            await Task.CompletedTask;
#endif
        }
    }
}
