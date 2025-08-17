
namespace TruckSlip.ViewModels
{
    public partial class CompanyViewModel : BaseViewModel
    {
        [ObservableProperty] private Company _selectedCompany = new();

        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;
        private int _index = -1;

        public CompanyViewModel(IDataServiceProvider provider)
        {
            _provider = provider;
            RefreshButtonState();
        }

        [RelayCommand]
        public async Task Appearing()
        {
            SetButtonText(false);
            if (!await RefreshCompanyAsync(Database))
            {
                EnableDelete = EnableEdit = false;
                return;
            }

            SelectedCompany = Companies.FirstOrDefault() ?? new();
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
                    view.Focus();
                }
                else
                {
                    VerifyInput();

                    if (await Database.AddOrUpdateCompanyAsync(SelectedCompany))
                    {
                        SetButtonText(false);
                        var companyId = SelectedCompany.CompanyId;
                        await RefreshCompanyAsync(Database);
                        SelectedCompany = Companies.FirstOrDefault(x=> x.CompanyId == companyId) 
                            ?? throw new Exception("Company not found!");
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
                    _index = Companies.IndexOf(SelectedCompany);
                    SelectedCompany = new();
                    EnableEdit = true;
                    view.Focus();
                }
                else
                {
                    SetButtonText(false);
                    EnableDelete = EnableEdit = await RefreshCompanyAsync(Database);

                    if (_index > -1)
                        SelectedCompany = Companies[_index];
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
                _index = Companies.IndexOf(SelectedCompany);

                if (!await Shell.Current.DisplayAlert("WARNING!",
                    "Deleting this Company also deletes all jobs and orders for this company, " +
                    "are you sure you want to proceed? This action can not be undone.",
                    "Yes", "No")) return;

                await Database.DeleteCompanyAsync(SelectedCompany);
                EnableDelete = EnableEdit = await RefreshCompanyAsync(Database);

                if (Companies.Count == 0)
                    return;

                if (_index > Companies.IndexOf(Companies.Last()))
                    SelectedCompany = Companies.Last();
                else
                    SelectedCompany = Companies[_index];

            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }

            finally
            { _index = -1; }
        }

        private void VerifyInput()
        {
            if (string.IsNullOrWhiteSpace(SelectedCompany.Name)) throw new Exception("Company name can not be blank");

            var result = Companies.Where(x => x.Name == SelectedCompany.Name).Any();

            if (SelectedCompany.CompanyId == 0 && result)
            {
                throw new Exception($"The Company \"{SelectedCompany.Name}\" already exists. " +
                                    $"Company names must be unique.");
            }
        }

        [RelayCommand]
        public async Task UploadLogo()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Upload Logo Image",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null) return;

            using var stream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            SelectedCompany.Logo = memoryStream.ToArray();
            OnPropertyChanged(nameof(SelectedCompany));
        }
    }
}
