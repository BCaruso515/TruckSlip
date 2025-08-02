namespace TruckSlip.ViewModels
{
    public partial class JobsiteViewModel: BaseViewModel
    {
        [ObservableProperty] private Jobsite _selectedJobsite = new();

        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;

        public JobsiteViewModel(IDataServiceProvider provider)
        {
            _provider = provider;
            RefreshButtonState();
        }

        [RelayCommand]
        public async Task Appearing()
        {
            SetButtonText(false);
            
            if (! await RefreshJobsiteAsync(Database))
            {
                SetButtonText(true);
                return;
            }

            SelectedJobsite = Jobsites.First();
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

                    if (await Database.AddOrUpdateJobsiteAsync(SelectedJobsite))
                    {
                        SetButtonText(false);
                        await RefreshJobsiteAsync(Database);
                        SelectedJobsite = Jobsites.First();
                    }
                }
            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }
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
            using Stream stream = await result.OpenReadAsync();

            MemoryStream memoryStream = new();
            stream.CopyTo(memoryStream);
            SelectedJobsite.Logo = memoryStream.ToArray();
            OnPropertyChanged(nameof(SelectedJobsite));
        }

        private void VerifyInput()
        {
            if (string.IsNullOrWhiteSpace(SelectedJobsite.Name)) throw new Exception("Jobsite name can not be blank");
            return;
        }
    }
}
