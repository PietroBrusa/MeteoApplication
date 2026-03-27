using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MeteoApp.Models
{
    public class WeatherImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int weatherCode)
            {
                return WeatherCodeMapper.GetImageSource(weatherCode);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
