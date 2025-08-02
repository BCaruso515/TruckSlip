using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace TruckSlip.Controls;

public partial class SignaturePad : Popup
{
    public ObservableCollection<IDrawingLine> Lines { get; set; } = new ObservableCollection<IDrawingLine>();

    public SignaturePad()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void ClearButton_Clicked(object sender, EventArgs e)
        => DrawBoard.Lines.Clear();


    private async void SubmitButton_Clicked(object sender, EventArgs e)
    {
        using var stream = await DrawBoard.GetImageStream(1920, 1080);
        await CloseAsync(stream);
    }
}
