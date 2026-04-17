using MeteoApp.Models;
using SQLite;

namespace MeteoApp
{
    public class MeteoLocation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public bool NotificationsEnabled { get; set; }

        public double CurrentTemperature { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public int WeatherCode { get; set; }
        public string WeatherDescription { get; set; }

        // False for the GPS current-location entry, which cannot be deleted by the user
        public bool IsDeletable { get; set; } = true;

        // Computed display properties — no setter so SQLite-net ignores them
        public string TempDisplay => SettingsService.FormatTemperature(CurrentTemperature);
        public string TempMinDisplay => SettingsService.FormatTemperature(TempMin);
        public string TempMaxDisplay => SettingsService.FormatTemperature(TempMax);
    }
}
