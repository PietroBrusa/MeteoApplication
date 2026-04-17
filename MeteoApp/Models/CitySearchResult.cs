namespace MeteoApp.Models
{
    // Represents one result from the OpenWeatherMap Geocoding API (/geo/1.0/direct)
    public class CitySearchResult
    {
        public string name { get; set; }
        public string local_names { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string country { get; set; }
        public string state { get; set; }

        // Human-readable label shown in the search results list
        public string DisplayName => string.IsNullOrEmpty(state)
            ? $"{name}, {country}"
            : $"{name}, {state}, {country}";
    }
}
