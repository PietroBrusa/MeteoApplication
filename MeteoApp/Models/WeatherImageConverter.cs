using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MeteoApp.Models
{
    public class WeatherImageConverter : IValueConverter
    {
        private static readonly ConcurrentDictionary<WeatherCondition, ImageSource> _imageCache = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int weatherCode)
            {
                WeatherCondition condition = WeatherCodeMapper.GetConditionFromCode((int) value);
                return _imageCache.GetOrAdd(condition, condition =>
                {
                    var imagePath = WeatherCodeMapper.GetImageName((int) value);
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
