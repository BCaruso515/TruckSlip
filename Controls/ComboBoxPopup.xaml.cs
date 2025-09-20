using System.Collections;

namespace TruckSlip.Controls;

public partial class ComboBoxPopup : Popup<object>
{
    public ComboBoxPopup(IEnumerable itemSource, DataTemplate itemTemplate)
    {
        InitializeComponent();

       cvResults.ItemsSource = itemSource;
        cvResults.ItemTemplate = itemTemplate;
    }

    private async void ComboBoxPopup_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = e.CurrentSelection?.FirstOrDefault();
        if (selectedItem is null)
        {
            await CloseAsync(); // No selection
            return;
        }
        
        await Task.Delay(100); // Small delay to allow UI to update
        await CloseAsync(selectedItem);
    }
}
