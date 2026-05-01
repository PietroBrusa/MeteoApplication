using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace MeteoApp.Models
{
    /// <summary>
    /// XAML <see cref="IValueConverter"/> that turns an OWM weather code (int) into an
    /// <see cref="ImageSource"/>. Caches results per <see cref="WeatherCondition"/> to
    /// avoid recreating image sources on every cell render.
    /// </summary>
    public class WeatherImageConverter : IValueConverter
    {
        private static readonly ConcurrentDictionary<WeatherCondition, ImageSource> _imageCache = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int weatherCode)
            {
                WeatherCondition condition = WeatherCodeMapper.GetConditionFromCode(weatherCode);
                return _imageCache.GetOrAdd(condition, _ =>
                {
                    var imagePath = WeatherCodeMapper.GetImageName(weatherCode);
                    return ImageSource.FromFile(imagePath);
                });
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
