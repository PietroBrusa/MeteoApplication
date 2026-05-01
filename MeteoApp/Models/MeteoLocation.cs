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
        public double TempThresholdMin { get; set; } = 0;
        public double TempThresholdMax { get; set; } = 30;
        public bool IsDeletable { get; set; } = true;
        public string AppwriteDocumentId { get; set; } = string.Empty;

        public double CurrentTemperature { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public int WeatherCode { get; set; }
        public string WeatherDescription { get; set; }

        public double WindSpeed { get; set; }
        public int Humidity { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }

        public string TempDisplay => SettingsService.FormatTemperature(CurrentTemperature);
        public string TempMinDisplay => SettingsService.FormatTemperature(TempMin);
        public string TempMaxDisplay => SettingsService.FormatTemperature(TempMax);
        public string WindSpeedDisplay => $"{WindSpeed * 3.6:0.0} km/h";
        public string HumidityDisplay => $"{Humidity}%";
        public string SunriseDisplay => Sunrise.ToLocalTime().ToString("HH:mm");
        public string SunsetDisplay => Sunset.ToLocalTime().ToString("HH:mm");
    }
}
