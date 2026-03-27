using System;
using System.Collections.Generic;
using System.Text;

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
                // Thunderstorm (200-299)
                >= 200 and <= 299 => WeatherCondition.Thunderstorm,

                // Drizzle (300-399)
                >= 300 and <= 399 => WeatherCondition.Drizzle,

                // Rain (500-599)
                >= 500 and <= 599 => WeatherCondition.Rain,

                // Snow (600-699)
                >= 600 and <= 699 => WeatherCondition.Snow,

                // Atmosphere (700-799) - Mist, Fog, etc.
                >= 700 and <= 799 => WeatherCondition.Fog,

                // Clear (800)
                800 => WeatherCondition.Clear,

                // Clouds (801-899)
                >= 801 and <= 899 => code switch
                {
                    804 => WeatherCondition.Overcast,
                    _ => WeatherCondition.Clouds
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
                WeatherCondition.Drizzle => "rainy.png",
                WeatherCondition.Rain => "rainy.png",
                WeatherCondition.Snow => "rainy.png",
                WeatherCondition.Fog => "overcast.png",
                WeatherCondition.Clear => "sunny_clear.png",
                WeatherCondition.Clouds => "sunny_cloudy.png",
                WeatherCondition.Overcast => "overcast.png",
                _ => "default_weather.png"
            };

            return $"Weather/{filename}";
        }

        public static ImageSource GetImageSource(int code)
        {
            return ImageSource.FromFile(GetImageName(code));
        }
    }
}
