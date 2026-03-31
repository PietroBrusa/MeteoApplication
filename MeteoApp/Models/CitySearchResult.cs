namespace MeteoApp.Models
{
    public class CitySearchResult
    {
        public string name { get; set; }
        public string local_names { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string country { get; set; }
        public string state { get; set; }

        public string DisplayName => string.IsNullOrEmpty(state)
            ? $"{name}, {country}"
            : $"{name}, {state}, {country}";
    }
}