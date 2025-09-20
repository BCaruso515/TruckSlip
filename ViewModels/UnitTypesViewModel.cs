namespace TruckSlip.ViewModels
{
    public partial class UnitTypesViewModel : BaseViewModel
    {
        [ObservableProperty] private UnitType _selectedUnit = new();
        private int _index = -1;
        private bool _isPageInitialized = false;
        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;

        public UnitTypesViewModel(IDataServiceProvider provider)
        {
            _provider = provider;
            RefreshButtonState();
        }

        [RelayCommand]
        public async Task Appearing()
        {
            if (_isPageInitialized) return;
            SetButtonText(false);
            if (! await RefreshUnitTypesAsync(Database))
            {
                EnableDelete = EnableEdit = false;
                return;
            }

            SelectedUnit = UnitTypes.FirstOrDefault() ?? new();
            EnableDelete = EnableEdit =  true;
            _isPageInitialized = true;
        }

        [RelayCommand]
        public async Task EditSave(View view)
        {
            try
            {
                if (EditSaveButtonText == "Edit")
                {
                    SetButtonText(true);
                    _index = UnitTypes.IndexOf(SelectedUnit);
                    view.Focus();
                }
                else
                {
                    VerifyInput();

                    if (await Database.AddOrUpdateUnitTypeAsync(SelectedUnit))
                    {
                        SetButtonText(false);
                        EnableDelete = EnableEdit = await RefreshUnitTypesAsync(Database);
                        SelectedUnit = UnitTypes.FirstOrDefault(x => x.UnitId == SelectedUnit.UnitId)
                            ?? throw new Exception ("Unit type not found!");
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
                    _index = UnitTypes.IndexOf(SelectedUnit);
                    SelectedUnit = new();
                    EnableEdit = true;
                    view.Focus();
                }
                else
                {
                    SetButtonText(false);
                    EnableDelete = EnableEdit = await RefreshUnitTypesAsync(Database);

                    if (_index > -1)
                        SelectedUnit = UnitTypes[_index];
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
                _index = UnitTypes.IndexOf(SelectedUnit);

                if (!await Shell.Current.DisplayAlert("WARNING!",
                    "Deleting this Unit Type also deletes all items of this type, " +
                    "are you sure you want to proceed? This action can not be undone.",
                    "Yes", "No")) return;

                await Database.DeleteUnitTypeAsync(SelectedUnit);
                EnableDelete = EnableEdit = await RefreshUnitTypesAsync(Database);

                if (UnitTypes.Count == 0)
                    return;

                if (_index > UnitTypes.IndexOf(UnitTypes.Last()))
                    SelectedUnit = UnitTypes.Last();
                else
                    SelectedUnit = UnitTypes[_index];

            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }

            finally
            { _index = -1; }
        }

        private void VerifyInput()
        {
            if (string.IsNullOrWhiteSpace(SelectedUnit.UnitName)) throw new Exception("Unit name can not be blank");

            var result = UnitTypes.Where(x => x.UnitName == SelectedUnit.UnitName).Any();

            if (SelectedUnit.UnitId == 0 && result)
            {
                throw new Exception($"The unit \"{SelectedUnit.UnitName}\" already exists. " +
                                    $"Unit type names must be unique.");
            }
        }
    }
}
