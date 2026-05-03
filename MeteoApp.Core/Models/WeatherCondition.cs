namespace MeteoApp.Models
{
    public enum WeatherCondition
    {
        Thunderstorm,
        Drizzle,
        Rain,
        Snow,
        Fog,
        Clear,
        Clouds,
        Overcast,
        Unknown
    }

    public static class WeatherCodeMapper
    {
        public static WeatherCondition GetConditionFromCode(int code)
        {
            return code switch
            {
                >= 200 and <= 299 => WeatherCondition.Thunderstorm,
                >= 300 and <= 399 => WeatherCondition.Drizzle,
                >= 500 and <= 599 => WeatherCondition.Rain,
                >= 600 and <= 699 => WeatherCondition.Snow,
                >= 700 and <= 799 => WeatherCondition.Fog,
                800               => WeatherCondition.Clear,
                >= 801 and <= 899 => code switch
                {
                    804 => WeatherCondition.Overcast,
                    _   => WeatherCondition.Clouds
                },
                _ => WeatherCondition.Unknown
            };
        }

        public static string GetImageName(int code)
        {
            var condition = GetConditionFromCode(code);
            var filename = condition switch
            {
                WeatherCondition.Thunderstorm => "storm.png",
                WeatherCondition.Drizzle      => "rainy.png",
                WeatherCondition.Rain         => "rainy.png",
                WeatherCondition.Snow         => "rainy.png",
                WeatherCondition.Fog          => "overcast.png",
                WeatherCondition.Clear        => "sunny_clear.png",
                WeatherCondition.Clouds       => "sunny_cloudy.png",
                WeatherCondition.Overcast     => "overcast.png",
                _                             => "default_weather.png"
            };

            return $"Weather/{filename}";
        }
    }
}
