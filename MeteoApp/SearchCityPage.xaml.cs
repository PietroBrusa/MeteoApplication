using MeteoApp.Resources.Strings;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MeteoApp;

public partial class SearchCityPage : ContentPage
{
    private MeteoListViewModel _viewModel;
    private MeteoLocation _selectedMapLocation;

    public SearchCityPage(MeteoListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;

        // Default map view centered on Switzerland
        var defaultLocation = new Location(46.8, 8.2);
        SelectionMap.MoveToRegion(MapSpan.FromCenterAndRadius(defaultLocation, Distance.FromKilometers(400)));
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSearchPressed(object sender, EventArgs e)
    {
        string query = CitySearchBar.Text;
        if (!string.IsNullOrWhiteSpace(query))
        {
            var results = await _viewModel.SearchCitiesAsync(query);

            if (results != null && results.Count > 0)
            {
                ResultsCollectionView.ItemsSource = results;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    AppResources.NoCityFoundTitle,
                    AppResources.NoCityFoundMessage,
                    AppResources.OkButton);
                ResultsCollectionView.ItemsSource = null;
            }
        }
    }

    private async void OnCitySelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedCity = e.CurrentSelection?.FirstOrDefault() as MeteoLocation;

        if (selectedCity != null)
        {
            await _viewModel.AddCityAsync(selectedCity.Name);
            await Navigation.PopModalAsync();
        }

        // Clear selection so the same item can be tapped again
        if (sender is CollectionView cv)
        {
            cv.SelectedItem = null;
        }
    }

    // Handles a tap on the map: reverse geocodes the coordinates to a city name
    private async void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        try
        {
            var placemarks = await Geocoding.GetPlacemarksAsync(
                e.Location.Latitude,
                e.Location.Longitude);
            var placemark = placemarks?.FirstOrDefault();
            if (placemark == null) return;

            // Use the most specific available place name
            string cityName = placemark.Locality
                ?? placemark.SubAdminArea
                ?? placemark.AdminArea
                ?? placemark.CountryName;

            if (string.IsNullOrEmpty(cityName)) return;

            _selectedMapLocation = await _viewModel.FetchWeatherForCityAsync(cityName, 0);
            if (_selectedMapLocation.WeatherDescription == "Error loading data") return;

            // Drop a pin and show the confirm bar
            SelectionMap.Pins.Clear();
            SelectionMap.Pins.Add(new Pin
            {
                Label = _selectedMapLocation.Name,
                Location = e.Location
            });

            SelectedCityLabel.Text = _selectedMapLocation.Name;
            ConfirmFrame.IsVisible = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Map selection error: {ex.Message}");
        }
    }

    // Saves the map-selected city and dismisses the page
    private async void OnAddFromMapClicked(object sender, EventArgs e)
    {
        if (_selectedMapLocation != null)
        {
            // try/catch required in async void event handlers
            try
            {
                await _viewModel.AddCityAsync(_selectedMapLocation.Name);
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, AppResources.OkButton);
            }
        }
    }
}
