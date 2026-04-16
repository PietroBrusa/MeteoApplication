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
        public bool IsDeletable { get; set; } = true;

        // Proprietà di visualizzazione calcolate (nessun setter = ignorate da SQLite-net-pcl)
        public string TempDisplay => SettingsService.FormatTemperature(CurrentTemperature);
        public string TempMinDisplay => SettingsService.FormatTemperature(TempMin);
        public string TempMaxDisplay => SettingsService.FormatTemperature(TempMax);
    }
}