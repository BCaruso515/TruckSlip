namespace TruckSlip.Controls;

public partial class DescriptionBox : ContentView
{
    public static readonly BindableProperty Text1Property = BindableProperty.Create(
        propertyName: nameof(Text1),
        returnType: typeof(string),
        declaringType: typeof(DescriptionBox),
        propertyChanged: Text1PropertyChanged,
        defaultBindingMode: BindingMode.OneWay);

    public string Text1
    {
        get => (string)GetValue(Text1Property);
        set => SetValue(Text1Property, value);
    }

    private static void Text1PropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var controls = (DescriptionBox)bindable;

        controls.label1.Text = (string)newValue;
    }

    public static readonly BindableProperty Text2Property = BindableProperty.Create(
        propertyName: nameof(Text2),
        returnType: typeof(string),
        declaringType: typeof(DescriptionBox),
        propertyChanged: Text2PropertyChanged,
        defaultBindingMode: BindingMode.OneWay);

    public string Text2
    {
        get => (string)GetValue(Text2Property);
        set => SetValue(Text2Property, value);
    }

    private static void Text2PropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var controls = (DescriptionBox)bindable;

        controls.label2.Text = (string)newValue;
    }

    public static readonly BindableProperty Text3Property = BindableProperty.Create(
        propertyName: nameof(Text3),
        returnType: typeof(string),
        declaringType: typeof(DescriptionBox),
        propertyChanged: Text3PropertyChanged,
        defaultBindingMode: BindingMode.OneWay);

    public string Text3
    {
        get => (string)GetValue(Text3Property);
        set => SetValue(Text3Property, value);
    }

    private static void Text3PropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var controls = (DescriptionBox)bindable;

        controls.label3.Text = (string)newValue;
    }

    public static readonly BindableProperty Text4Property = BindableProperty.Create(
        propertyName: nameof(Text4),
        returnType: typeof(string),
        declaringType: typeof(DescriptionBox),
        propertyChanged: Text4PropertyChanged,
        defaultBindingMode: BindingMode.OneWay);

    public string Text4
    {
        get => (string)GetValue(Text4Property);
        set => SetValue(Text4Property, value);
    }

    private static void Text4PropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var controls = (DescriptionBox)bindable;

        controls.label4.Text = (string)newValue;
    }

    public static readonly BindableProperty Text5Property = BindableProperty.Create(
        propertyName: nameof(Text5),
        returnType: typeof(string),
        declaringType: typeof(DescriptionBox),
        propertyChanged: Text5PropertyChanged,
        defaultBindingMode: BindingMode.OneWay);

    public string Text5
    {
        get => (string)GetValue(Text5Property);
        set => SetValue(Text5Property, value);
    }

    private static void Text5PropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var controls = (DescriptionBox)bindable;

        controls.label5.Text = (string)newValue;
    }

    private bool ShowDescription { get; set; }

    public DescriptionBox()
	{
		InitializeComponent();
        ShowDescription = false;
        descriptionBox.IsVisible = false;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        ShowDescription = !ShowDescription;
        
        showButton.IsVisible = !ShowDescription;
        descriptionBox.IsVisible = ShowDescription;
    }
}