using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MeteoApp.Models
{
    // IValueConverter that turns a WeatherCode (int) into an ImageSource for XAML bindings
    public class WeatherImageConverter : IValueConverter
    {
        // Cache ImageSource objects per condition to avoid recreating them on every cell render
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

        // One-way binding only — back-conversion is not needed
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
