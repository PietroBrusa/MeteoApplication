using System.Collections.ObjectModel;
using MeteoApp.Resources.Strings;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService = new SettingsService();
        private readonly WeatherApiService _weatherApi;
        private readonly NotificationService _notificationService;
        private readonly AppwriteSyncService _appwriteSync;

        ObservableCollection<MeteoLocation> _entries;

        public ObservableCollection<MeteoLocation> Entries
        {
            get { return _entries; }
            set { _entries = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public MeteoListViewModel()
        {
            _weatherApi = IPlatformApplication.Current!.Services.GetRequiredService<WeatherApiService>();
            _notificationService = IPlatformApplication.Current!.Services.GetRequiredService<NotificationService>();
            _appwriteSync = IPlatformApplication.Current!.Services.GetRequiredService<AppwriteSyncService>();
            Entries = new ObservableCollection<MeteoLocation>();
        }

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
                    if (updatedLoc.WeatherDescription != "Error loading data")
                    {
                        updatedLoc.NotificationsEnabled = loc.NotificationsEnabled;
                        updatedLoc.TempThresholdMin = loc.TempThresholdMin;
                        updatedLoc.TempThresholdMax = loc.TempThresholdMax;
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

        public async Task ToggleTemperatureUnitAsync()
        {
            string newUnit = SettingsService.CurrentUnit == "C" ? "F" : "C";
            _settingsService.SaveTemperatureUnit(newUnit);
            await LoadLocationsFromDatabaseAsync();
        }

        public async Task AddCityAsync(string cityName)
        {
            var newLocation = await _weatherApi.FetchWeatherForCityAsync(cityName, 0);

            if (newLocation.WeatherDescription != "Error loading data")
            {
                await App.Database.SaveLocationAsync(newLocation);
                Entries.Add(newLocation);

                var token = _notificationService.GetCachedToken();
                var docId = await _appwriteSync.SyncLocationAsync(newLocation, token);
                if (!string.IsNullOrEmpty(docId) && docId != newLocation.AppwriteDocumentId)
                {
                    newLocation.AppwriteDocumentId = docId;
                    await App.Database.UpdateLocationAsync(newLocation);
                }
            }
        }

        public async Task RemoveCityAsync(int id)
        {
            var locationToRemove = Entries.FirstOrDefault(l => l.Id == id);
            if (locationToRemove != null && !string.IsNullOrEmpty(locationToRemove.AppwriteDocumentId))
                await _appwriteSync.DeleteLocationAsync(locationToRemove.AppwriteDocumentId);

            await App.Database.DeleteLocationAsync(id);
            if (locationToRemove != null)
                Entries.Remove(locationToRemove);
        }

        public Task<MeteoLocation> FetchWeatherForCityAsync(string cityName, int id)
            => _weatherApi.FetchWeatherForCityAsync(cityName, id);

        public Task<List<MeteoLocation>> SearchCitiesAsync(string query)
            => _weatherApi.SearchCitiesAsync(query);

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
