using Android.Graphics;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MeteoApp;

// QueryProperty lets shell navigation pass MeteoLocation as a page parameter
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
}
