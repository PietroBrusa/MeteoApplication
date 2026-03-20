using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        ObservableCollection<MeteoLocation> _entries;

        public ObservableCollection<MeteoLocation> Entries
        {
            get { return _entries; }
            set
            {
                _entries = value;
                OnPropertyChanged();
            }
        }

        public MeteoListViewModel()
        {
            Entries = new ObservableCollection<MeteoLocation>();

        }

        public async Task LoadLocationsFromDatabaseAsync()
        {
            Entries.Clear();
            string currentCityName = await GetGPSCityNameAsync();

            if (!currentCityName.Contains("denied") && !currentCityName.Contains("error") && !currentCityName.Contains("disabled"))
            {
                var currentLocationWeather = await FetchWeatherForCityAsync(currentCityName, 0);
                currentLocationWeather.Name = "Current Location (" + currentLocationWeather.Name + ")";
                currentLocationWeather.IsDeletable = false;
                Entries.Add(currentLocationWeather);
            }

            var savedLocations = await App.Database.GetLocationsAsync();
            foreach (var loc in savedLocations)
            {
                Entries.Add(loc);
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

        public async Task<MeteoLocation> FetchWeatherForCityAsync(string cityName, int id)
        {
            using HttpClient client = new HttpClient();

            string url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={Secret.OpenWeatherMapApiKey}&units=metric";

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
                };
            }
            catch (Exception ex)
            {
                return new MeteoLocation { Id = id, Name = cityName, WeatherDescription = "Error loading data" };
            }
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

                    if (placemark != null)
                    {
                        return $"{placemark.Locality}";
                    }
                }
                return "Invalid Location";
            }
            catch (FeatureNotEnabledException)
            {
                return "GPS is disabled on device";
            }
            catch (PermissionException)
            {
                return "Location permission denied";
            }
            catch (Exception ex)
            {
                return $"Location error: {ex.Message}";
            }
        } 
    }
}