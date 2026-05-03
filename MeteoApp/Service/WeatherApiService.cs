using System.Globalization;
using Newtonsoft.Json;

namespace MeteoApp;

public class WeatherApiService
{
    private readonly HttpClient _client = new();
    private const string BaseUrl = "https://api.openweathermap.org/data/2.5";

    private static string CurrentLang =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    public async Task<MeteoLocation> FetchWeatherForCityAsync(string cityName, int id)
    {
        string encodedCity = Uri.EscapeDataString(cityName);
        string url = $"{BaseUrl}/weather?q={encodedCity}&appid={Secret.OpenWeatherMapApiKey}&units=metric&lang={CurrentLang}";

        try
        {
            string responseJson = await _client.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<WeatherResponse>(responseJson);

            return new MeteoLocation
            {
                Id = id,
                Name = data.name,
                CurrentTemperature = data.main.temp,
                TempMin = data.main.temp_min,
                TempMax = data.main.temp_max,
                WeatherDescription = data.weather[0].description,
                WeatherCode = data.weather[0].id,
                Latitude = data.coord.lat,
                Longitude = data.coord.lon,
                WindSpeed = data.wind.speed,
                Humidity = data.main.humidity,
                Sunrise = DateTimeOffset.FromUnixTimeSeconds(data.sys.sunrise).UtcDateTime,
                Sunset = DateTimeOffset.FromUnixTimeSeconds(data.sys.sunset).UtcDateTime,
            };
        }
        catch
        {
            return new MeteoLocation { Id = id, Name = cityName, WeatherDescription = "Error loading data" };
        }
    }

    public async Task<List<MeteoLocation>> SearchCitiesAsync(string query)
    {
        var results = new List<MeteoLocation>();
        string encodedCity = Uri.EscapeDataString(query);
        string url = $"{BaseUrl}/find?q={encodedCity}&appid={Secret.OpenWeatherMapApiKey}&units=metric&lang={CurrentLang}";

        try
        {
            string responseJson = await _client.GetStringAsync(url);
            var findData = JsonConvert.DeserializeObject<WeatherFindResponse>(responseJson);

            if (findData?.list != null)
            {
                foreach (var data in findData.list)
                {
                    string country = !string.IsNullOrEmpty(data.sys?.country) ? $", {data.sys.country}" : "";

                    results.Add(new MeteoLocation
                    {
                        Id = data.id,
                        Name = data.name + country,
                        CurrentTemperature = data.main.temp,
                        WeatherDescription = data.weather[0].description,
                        WeatherCode = data.weather[0].id,
                        Latitude = data.coord.lat,
                        Longitude = data.coord.lon
                    });
                }
            }
        }
        catch (Exception) { }

        return results;
    }
}
