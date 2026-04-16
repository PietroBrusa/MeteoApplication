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

        // Centra la mappa sulla Svizzera come posizione di default
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

        if (sender is CollectionView cv)
        {
            cv.SelectedItem = null;
        }
    }

    // Gestisce il tap sulla mappa (AS5 - MapClicked + geocoding inverso)
    private async void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        try
        {
            // Geocoding inverso: coordinate → indirizzo (AS5)
            var placemarks = await Geocoding.GetPlacemarksAsync(
                e.Location.Latitude,
                e.Location.Longitude);
            var placemark = placemarks?.FirstOrDefault();
            if (placemark == null) return;

            // Prende il nome più specifico disponibile
            string cityName = placemark.Locality
                ?? placemark.SubAdminArea
                ?? placemark.AdminArea
                ?? placemark.CountryName;

            if (string.IsNullOrEmpty(cityName)) return;

            // Recupera i dati meteo per la città trovata
            _selectedMapLocation = await _viewModel.FetchWeatherForCityAsync(cityName, 0);
            if (_selectedMapLocation.WeatherDescription == "Error loading data") return;

            // Aggiorna il pin sulla mappa
            SelectionMap.Pins.Clear();
            SelectionMap.Pins.Add(new Pin
            {
                Label = _selectedMapLocation.Name,
                Location = e.Location
            });

            // Mostra la barra di conferma con il nome della città
            SelectedCityLabel.Text = _selectedMapLocation.Name;
            ConfirmFrame.IsVisible = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Map selection error: {ex.Message}");
        }
    }

    // Aggiunge la città selezionata dalla mappa alla lista
    private async void OnAddFromMapClicked(object sender, EventArgs e)
    {
        if (_selectedMapLocation != null)
        {
            // try/catch obbligatorio negli async void handler (AS3)
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
