using static Android.Security.Identity.CredentialDataResult;

namespace MeteoApp;

[QueryProperty(nameof(MeteoLocation), "MeteoLocation")]
public partial class MeteoItemPage : ContentPage
{
    MeteoLocation meteoLocation;

    public MeteoLocation MeteoLocation
    {
        get => meteoLocation;
        set
        {
            meteoLocation = value;
            OnPropertyChanged();
        }
    }

    public MeteoItemPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}