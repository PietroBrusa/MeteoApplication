using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MeteoApp;

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
            UpdateMap();
        }
    }

    public MeteoItemPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // Centers the map on the city and drops a pin when the location is set
    private void UpdateMap()
    {
        if (MeteoLocation != null && CityMap != null)
        {
            var location = new Location(MeteoLocation.Latitude, MeteoLocation.Longitude);

            CityMap.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(10)));

            CityMap.Pins.Clear();
            CityMap.Pins.Add(new Pin
            {
                Label = MeteoLocation.Name,
                Location = location
            });
        }
    }

    private async void OnLocationNameTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new MeteoDetailPage(MeteoLocation));
    }

    // Opens the per-location notification settings page (thresholds + toggle).
    // Disabled for the GPS entry since it isn't persisted in the local DB.
    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        if (MeteoLocation == null || !MeteoLocation.IsDeletable) return;
        await Navigation.PushAsync(new LocationSettingsPage(MeteoLocation));
    }
}
