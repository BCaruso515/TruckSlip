
namespace TruckSlip.ViewModels
{
    //[QueryProperty(nameof(FormId), nameof(FormId))]
    public partial class SignFormViewModel : BaseViewModel
    {
    //    [ObservableProperty] private Person selectedPerson = new();
    //    [ObservableProperty] private Form selectedForm = new();
    //    [ObservableProperty] private int formId;

    //    private ObservableCollection<Person> _people = [];
    //    private readonly IDataServiceProvider _provider;
    //    private IInventoryDataService Database => _provider.Current;

    //    public SignFormViewModel(IDataServiceProvider provider)
    //    {
    //        _provider = provider;
    //    }

    //    [RelayCommand]
    //    public async Task Appearing() 
    //    {
    //        if (!await RefreshPeopleAsync(Database)) return;
    //        if (FormId == 0) return;
    //        if (!await RefreshFormAsync(Database)) return;

    //        SelectedForm = Forms.First(x => x.FormId == FormId);
    //        await GetPeopleWithoutSignatureAsync();
    //    }

    //    private async Task<bool> GetPeopleWithoutSignatureAsync()
    //    {
    //        await RefreshPeopleAsync(Database);
    //        People = [.. People.Where(x => x.Signature.Length == 0)];
    //        return People.Count != 0;
    //    }

    //    private async Task<bool> GetPeopleWithSignatureAsync()
    //    {
    //        _people = await Database.GetPersonAsync();
    //        _people = [.. _people.Where(x => x.Signature.Length != 0).OrderBy(x => x.FullName)];
    //        return _people.Count != 0;
    //    }

    //    [RelayCommand]
    //    private async Task GetSignatureFromPad()
    //    {
    //        var popup = new Controls.SignaturePad();
    //        var result = await Shell.Current.ShowPopupAsync(popup);
                        
    //        if(result is not Stream) return;

    //        SelectedPerson.Signature = ConvertToByteArray(result as Stream);
    //        if (!await Database.AddOrUpdatePersonAsync(SelectedPerson)) return;
    //        await Toast.Make($"You have successfully signed the {SelectedForm.FormName} form",
    //                         ToastDuration.Short,
    //                         16).Show();
            
    //        await GetPeopleWithoutSignatureAsync();
    //    }

    //    [RelayCommand]
    //    private async Task ViewPdf()
    //    {
    //        try
    //        {
    //            if (! await GetPeopleWithSignatureAsync())
    //            { 
    //                await Shell.Current.DisplayAlert("Missing Signatures", "Signature(s) not found", "Ok");
    //                return;
    //            }
                
    //            if (_people.Count > SelectedForm.MaximumSignatures)
    //               if(! await Shell.Current.DisplayAlert("Maximum records exceded", 
    //                   $"{_people.Count - SelectedForm.MaximumSignatures} signatures will be omitted from this form. Do you wish to continue?",
    //                   "Yes","No")) return;

    //            var outputFile = SignFormReport.GenerateSignatureReport(_people, SelectedForm);
    //            if (outputFile == string.Empty) return;
    //                await Launcher.Default.OpenAsync(new OpenFileRequest("Open document...", new ReadOnlyFile(outputFile)));
    //        }
    //        catch (Exception ex)
    //        {
    //          await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
    //        }
    //    }

    //    private static byte[] ConvertToByteArray(Stream? stream)
    //    {
    //        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
    //        using var memoryStream = new MemoryStream();
    //        stream.CopyTo(memoryStream);
    //        return memoryStream.ToArray();
    //    }
    }
}
