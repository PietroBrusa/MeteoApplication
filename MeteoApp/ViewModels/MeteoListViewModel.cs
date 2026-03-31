using System.Collections.ObjectModel;
using MeteoApp.Models;
using Newtonsoft.Json;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        ObservableCollection<MeteoLocation> _entries;

        public ObservableCollection<MeteoLocation> Entries
        {
            get { return _entries; }
            set { _entries = value; OnPropertyChanged(); }
        }

        public MeteoListViewModel()
        {
            Entries = new ObservableCollection<MeteoLocation>();
        }

        public async Task LoadLocationsFromDatabaseAsync()
        {
            Entries.Clear();

            var savedLocations = await App.Database.GetLocationsAsync();
            foreach (var loc in savedLocations)
            {
                var updatedLoc = await FetchWeatherForCityAsync(loc.Name, loc.Id);
                if (updatedLoc.WeatherDescription != "Error loading data")
                {
                    Entries.Add(updatedLoc);
                }
                else
                {
                    Entries.Add(loc);
                }
            }

            string currentCityName = await GetGPSCityNameAsync();

            if (!currentCityName.Contains("denied") && !currentCityName.Contains("error") && !currentCityName.Contains("disabled"))
            {
                var currentLocationWeather = await FetchWeatherForCityAsync(currentCityName, 0);
                currentLocationWeather.Name = "Current Location (" + currentLocationWeather.Name + ")";
                currentLocationWeather.IsDeletable = false;

                Entries.Insert(0, currentLocationWeather);
            }
        }

        public async Task AddCityAsync(string cityName)
        {
            var newLocation = await FetchWeatherForCityAsync(cityName, 0);

            if (newLocation.WeatherDescription != "Error loading data")
            {
                await App.Database.SaveLocationAsync(newLocation);
                Entries.Add(newLocation);
            }
        }

        public async Task RemoveCityAsync(int id)
        {
            await App.Database.DeleteLocationAsync(id);
            var locationToRemove = Entries.FirstOrDefault(l => l.Id == id);
            if (locationToRemove != null)
            {
                Entries.Remove(locationToRemove);
            }
        }

        // METODO CLASSICO PER SCARICARE UNA CITTÀ SPECIFICA (E SALVARLA)
        public async Task<MeteoLocation> FetchWeatherForCityAsync(string cityName, int id)
        {
            using HttpClient client = new HttpClient();

            string encodedCity = Uri.EscapeDataString(cityName);
            string lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            // Endpoint /weather
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={encodedCity}&appid={Secret.OpenWeatherMapApiKey}&units=metric&lang={lang}";

            try
            {
                string responseJson = await client.GetStringAsync(url);
                var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(responseJson);

                return new MeteoLocation
                {
                    Id = id,
                    Name = weatherData.name,
                    CurrentTemperature = weatherData.main.temp,
                    WeatherDescription = weatherData.weather[0].description,
                    WeatherCode = weatherData.weather[0].id,
                    Latitude = weatherData.coord.lat,
                    Longitude = weatherData.coord.lon
                };
            }
            catch (Exception ex)
            {
                return new MeteoLocation { Id = id, Name = cityName, WeatherDescription = "Error loading data" };
            }
        }

        // NUOVO METODO PER CERCARE LE OMONIMIE (PER LA SEARCH PAGE)
        public async Task<List<MeteoLocation>> SearchCitiesAsync(string query)
        {
            var results = new List<MeteoLocation>();
            using HttpClient client = new HttpClient();

            string encodedCity = Uri.EscapeDataString(query);
            string lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            // Endpoint /find
            string url = $"https://api.openweathermap.org/data/2.5/find?q={encodedCity}&appid={Secret.OpenWeatherMapApiKey}&units=metric&lang={lang}";

            try
            {
                string responseJson = await client.GetStringAsync(url);
                var findData = JsonConvert.DeserializeObject<WeatherFindResponse>(responseJson);

                if (findData != null && findData.list != null)
                {
                    foreach (var weatherData in findData.list)
                    {
                        string country = !string.IsNullOrEmpty(weatherData.sys?.country) ? $", {weatherData.sys.country}" : "";

                        results.Add(new MeteoLocation
                        {
                            Id = weatherData.id,
                            Name = weatherData.name + country,
                            CurrentTemperature = weatherData.main.temp,
                            WeatherDescription = weatherData.weather[0].description,
                            WeatherCode = weatherData.weather[0].id,
                            Latitude = weatherData.coord.lat,
                            Longitude = weatherData.coord.lon
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la ricerca: {ex.Message}");
            }

            return results;
        }

        public async Task<string> GetGPSCityNameAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                        return "Location permission denied";
                }

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                {
                    var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var placemark = placemarks?.FirstOrDefault();
                    if (placemark != null) return $"{placemark.Locality}";
                }
                return "Invalid Location";
            }
            catch (FeatureNotEnabledException) { return "GPS is disabled on device"; }
            catch (PermissionException) { return "Location permission denied"; }
            catch (Exception ex) { return $"Location error: {ex.Message}"; }
        }
    }
}