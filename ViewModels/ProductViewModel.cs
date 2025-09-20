namespace TruckSlip.ViewModels
{
    public partial class ProductViewModel : BaseViewModel
    {
        [ObservableProperty] private UnitType _selectedUnit = new();
        [ObservableProperty] private InventoryType _selectedType = new();
        [ObservableProperty] private Product _selectedProduct = new();

        private int _index = -1;
        private bool _isPageInitialized = false;
        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;

        public ProductViewModel(IDataServiceProvider provider)
        {
            _provider = provider;
            RefreshButtonState();
        }

        [RelayCommand]
        public async Task Appearing()
        {
            if (_isPageInitialized) return;
            SetButtonText(false);

            if (RefreshInventoryTypes())
                SelectedType = InventoryTypes.FirstOrDefault() ?? new();

            EnableAdd = await RefreshUnitTypesAsync(Database);
            if (!EnableAdd)
            {
                await Shell.Current.DisplayAlert("No Unit Types Found",
                    "You must add Unit Type before you can add an item.", "Ok");
                EnableDelete = EnableEdit = false;
                return;
            }

            SelectedUnit = UnitTypes.FirstOrDefault() ?? new();

            if (!await RefreshProductsAsync(Database))
            {
                EnableDelete = EnableEdit = false;
                return;
            }

            SelectedProduct = Products.FirstOrDefault() ?? new();
            EnableDelete = EnableEdit = true;
            _isPageInitialized = true;
        }

        [RelayCommand]
        public void SelectedProductChanged()
        {
            SetUnitType();
            SetInventoryType();
        }

        [RelayCommand]
        public async Task EditSave(View view)
        {
            try
            {
                if (EditSaveButtonText == "Edit")
                {
                    SetButtonText(true);
                    _index = Products.IndexOf(SelectedProduct);
                    view.Focus();
                }
                else
                {
                    SelectedProduct.UnitId = SelectedUnit.UnitId;
                    SelectedProduct.TypeId = SelectedType.TypeId;
                    if (await Database.AddOrUpdateProductAsync(SelectedProduct))
                    {
                        SetButtonText(false);
                        EnableDelete = EnableEdit = await RefreshProductsAsync(Database);
                        SelectedProduct = Products.FirstOrDefault(x => x.ProductId == SelectedProduct.ProductId)
                            ?? throw new Exception ("Product not found!");
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
                    _index = Products.IndexOf(SelectedProduct);
                    SelectedProduct = new();
                    EnableEdit = true;
                    view.Focus();
                }
                else
                {
                    SetButtonText(false);
                    EnableDelete = EnableEdit = await RefreshProductsAsync(Database);

                    if (_index > -1)
                        SelectedProduct = Products[_index];
                    _index = -1;
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
                _index = Products.IndexOf(SelectedProduct);

                if (!await Shell.Current.DisplayAlert("WARNING!",
                    "Deleting this Product also deletes all records containing this Product, " +
                    "are you sure you want to proceed? This action can not be undone.",
                    "Yes", "No")) return;

                await Database.DeleteProductAsync(SelectedProduct);
                EnableDelete = EnableEdit = await RefreshProductsAsync(Database);

                if (Products.Count == 0)
                    return;

                if (_index > Products.IndexOf(Products.Last()))
                    SelectedProduct = Products.Last();
                else
                    SelectedProduct = Products[_index];

            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }

            finally
            { _index = -1; }
        }

        private void SetUnitType()
        {
            if (SelectedProduct.UnitId == 0) return;
            var result = UnitTypes.FirstOrDefault(x => x.UnitId == SelectedProduct.UnitId);
            if (result == null) return;
            SelectedUnit = result;
        }

        private void SetInventoryType()
        {
            if (SelectedProduct.TypeId == 0) return;
            var result = InventoryTypes.FirstOrDefault(x => x.TypeId == SelectedProduct.TypeId);
            if (result == null) return;
            SelectedType = result;
        }
    }
}
