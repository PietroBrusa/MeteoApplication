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
        public int WeatherCode { get; set; }
        public string WeatherDescription { get; set; }
        public bool IsDeletable { get; set; } = true;
    }
}