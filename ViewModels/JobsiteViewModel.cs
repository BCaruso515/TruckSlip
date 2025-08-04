using System.Linq;

namespace TruckSlip.ViewModels
{
    public partial class JobsiteViewModel: BaseViewModel
    {
        [ObservableProperty] private Jobsite _selectedJobsite = new();
        [ObservableProperty] private Company _selectedCompany = new();

        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;
        private int _index = -1;

        public JobsiteViewModel(IDataServiceProvider provider)
        {
            _provider = provider;
            RefreshButtonState();
        }
        
        [RelayCommand]
        public async Task SelectedCompanyChanged()
        { 
            if (! await RefreshJobsiteAsync(Database, SelectedCompany.CompanyId))
            {
                EnableDelete = EnableEdit = false;
                SelectedJobsite = new();
                return;
            }

            SelectedJobsite = Jobsites.First();
            EnableDelete = EnableEdit = true;
        }

        [RelayCommand]
        public async Task Appearing()
        {
            SetButtonText(false);
            EnableAdd = await RefreshCompanyAsync(Database);
            if (!EnableAdd)
            {
                await Shell.Current.DisplayAlert("No Companies Found",
                    "You must add a company before you can add a jobsite.", "Ok");
                EnableDelete = EnableEdit = false;
                return;
            }
            SelectedCompany = Companies.First();
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
                    SelectedJobsite.CompanyId = SelectedCompany.CompanyId;
                    if (await Database.AddOrUpdateJobsiteAsync(SelectedJobsite))
                    {
                        SetButtonText(false);
                        await RefreshJobsiteAsync(Database, SelectedCompany.CompanyId);
                        SelectedJobsite = Jobsites.First();
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
                    _index = Jobsites.IndexOf(SelectedJobsite);
                    SelectedJobsite = new();
                    EnableEdit = true;
                    view.Focus();
                }
                else
                {
                    SetButtonText(false);
                    EnableDelete = EnableEdit = await RefreshJobsiteAsync(Database, SelectedCompany.CompanyId);

                    if (_index > -1)
                        SelectedJobsite = Jobsites[_index];
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
                _index = Jobsites.IndexOf(SelectedJobsite);

                if (!await Shell.Current.DisplayAlert("WARNING!",
                    "Deleting this Jobsite also deletes all orders for this Jobsite, " +
                    "are you sure you want to proceed? This action can not be undone.",
                    "Yes", "No")) return;

                await Database.DeleteJobsiteAsync(SelectedJobsite);
                EnableDelete = EnableEdit = await RefreshJobsiteAsync(Database, SelectedCompany.CompanyId);

                if (Jobsites.Count == 0)
                    return;

                if (_index > Jobsites.IndexOf(Jobsites.Last()))
                    SelectedJobsite = Jobsites.Last();
                else
                    SelectedJobsite = Jobsites[_index];

            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }

            finally
            { _index = -1; }
        }

        private void VerifyInput()
        {
            if (string.IsNullOrWhiteSpace(SelectedJobsite.Name)) throw new Exception("Jobsite name can not be blank");

            var result = Jobsites.Where(x => x.Name == SelectedJobsite.Name).Any();

            if (SelectedJobsite.CompanyId == 0 && result)
            {
                throw new Exception($"The Jobsite \"{SelectedJobsite.Name}\" already exists. " +
                                    $"Jobsite names must be unique.");
            }
        }
    }
}
