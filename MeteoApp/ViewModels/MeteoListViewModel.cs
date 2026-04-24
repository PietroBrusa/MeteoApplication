using System.Collections.ObjectModel;
using MeteoApp.Models;
using MeteoApp.Resources.Strings;
using Newtonsoft.Json;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService = new SettingsService();

        ObservableCollection<MeteoLocation> _entries;

        public ObservableCollection<MeteoLocation> Entries
        {
            get { return _entries; }
            set { _entries = value; OnPropertyChanged(); }
        }

        // Controls ActivityIndicator visibility during async operations
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public MeteoListViewModel()
        {
            Entries = new ObservableCollection<MeteoLocation>();
        }

        // Loads saved cities from DB, refreshes their weather, then prepends the GPS location
        public async Task LoadLocationsFromDatabaseAsync()
        {
            IsLoading = true;

            try
            {
                Entries.Clear();

                var savedLocations = await App.Database.GetLocationsAsync();
                foreach (var loc in savedLocations)
                {
                    var updatedLoc = await FetchWeatherForCityAsync(loc.Name, loc.Id);
                    // Fall back to cached data if the API call fails
                    if (updatedLoc.WeatherDescription != "Error loading data")
                        Entries.Add(updatedLoc);
                    else
                        Entries.Add(loc);
                }

                string currentCityName = await GetGPSCityNameAsync();

                // Only insert GPS entry when permission and location are available
                if (!currentCityName.Contains("denied") && !currentCityName.Contains("error") && !currentCityName.Contains("disabled"))
                {
                    var currentLocationWeather = await FetchWeatherForCityAsync(currentCityName, 0);
                    currentLocationWeather.Name = string.Format(AppResources.CurrentLocation, currentLocationWeather.Name);
                    currentLocationWeather.IsDeletable = false;

                    Entries.Insert(0, currentLocationWeather);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Toggles °C / °F and reloads the list with the new unit
        public async Task ToggleTemperatureUnitAsync()
        {
            string newUnit = SettingsService.CurrentUnit == "C" ? "F" : "C";
            _settingsService.SaveTemperatureUnit(newUnit);
            await LoadLocationsFromDatabaseAsync();
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
                Entries.Remove(locationToRemove);
        }

        // Calls the /weather endpoint and maps the response to a MeteoLocation
        public async Task<MeteoLocation> FetchWeatherForCityAsync(string cityName, int id)
        {
            using HttpClient client = new HttpClient();

            string encodedCity = Uri.EscapeDataString(cityName);
            string lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

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
                    TempMin = weatherData.main.temp_min,
                    TempMax = weatherData.main.temp_max,
                    WeatherDescription = weatherData.weather[0].description,
                    WeatherCode = weatherData.weather[0].id,
                    Latitude = weatherData.coord.lat,
                    Longitude = weatherData.coord.lon,
                    WindSpeed = weatherData.wind.speed,
                    Humidity = weatherData.main.humidity,
                    Sunrise = DateTimeOffset.FromUnixTimeSeconds(weatherData.sys.sunrise).UtcDateTime,
                    Sunset = DateTimeOffset.FromUnixTimeSeconds(weatherData.sys.sunset).UtcDateTime,
                };
            }
            catch (Exception ex)
            {
                return new MeteoLocation { Id = id, Name = cityName, WeatherDescription = "Error loading data" };
            }
        }

        // Calls /find endpoint — returns multiple matches to resolve city name ambiguity
        public async Task<List<MeteoLocation>> SearchCitiesAsync(string query)
        {
            var results = new List<MeteoLocation>();
            using HttpClient client = new HttpClient();

            string encodedCity = Uri.EscapeDataString(query);
            string lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

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
                Console.WriteLine($"Search error: {ex.Message}");
            }

            return results;
        }

        // Requests location permission and resolves GPS coordinates to a city name
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
                    // Reverse geocoding: coordinates → placemark
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
