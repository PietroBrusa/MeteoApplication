using System.Collections.ObjectModel;
using System.Windows.Input;
using Newtonsoft.Json;
using MeteoApp.Models;

namespace MeteoApp.ViewModels
{
    public class SearchCityViewModel : BaseViewModel
    {
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CitySearchResult> SearchResults { get; set; } = new ObservableCollection<CitySearchResult>();

        public ICommand SearchCommand { get; }
        public ICommand SelectCityCommand { get; }

        public SearchCityViewModel()
        {
            SearchCommand = new Command(async () => await PerformSearchAsync());
            SelectCityCommand = new Command<CitySearchResult>(async (city) => await AddSelectedCityAsync(city));
        }

        private async Task PerformSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText) || SearchText.Length < 2)
                return;

            SearchResults.Clear();

            using HttpClient client = new HttpClient();
            string url = $"https://api.openweathermap.org/geo/1.0/direct?q={SearchText}&limit=5&appid={Secret.OpenWeatherMapApiKey}";

            try
            {
                string responseJson = await client.GetStringAsync(url);
                var results = JsonConvert.DeserializeObject<List<CitySearchResult>>(responseJson);

                if (results != null)
                {
                    foreach (var city in results)
                        SearchResults.Add(city);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore ricerca: {ex.Message}");
            }
        }

        private async Task AddSelectedCityAsync(CitySearchResult selectedCity)
        {
            if (selectedCity == null) return;

            using HttpClient client = new HttpClient();
            string langCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            string url = $"https://api.openweathermap.org/data/2.5/weather?lat={selectedCity.lat}&lon={selectedCity.lon}&appid={Secret.OpenWeatherMapApiKey}&units=metric&lang={langCode}";

            try
            {
                string responseJson = await client.GetStringAsync(url);
                var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(responseJson);

                var newLocation = new MeteoLocation
                {
                    Name = weatherData.name,
                    Latitude = weatherData.coord.lat,
                    Longitude = weatherData.coord.lon,
                    CurrentTemperature = weatherData.main.temp,
                    WeatherDescription = weatherData.weather[0].description,

                    WeatherCode = weatherData.weather[0].id
                };

                await App.Database.SaveLocationAsync(newLocation);

                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore salvataggio meteo: {ex.Message}");
            }
        }
    }
}