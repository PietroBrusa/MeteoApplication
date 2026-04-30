using MeteoApp.Models;

namespace MeteoApp.Tests
{
    public class WeatherCodeMapperTests
    {
        [Theory]
        [InlineData(200, WeatherCondition.Thunderstorm)]
        [InlineData(232, WeatherCondition.Thunderstorm)]
        [InlineData(300, WeatherCondition.Drizzle)]
        [InlineData(321, WeatherCondition.Drizzle)]
        [InlineData(500, WeatherCondition.Rain)]
        [InlineData(531, WeatherCondition.Rain)]
        [InlineData(600, WeatherCondition.Snow)]
        [InlineData(622, WeatherCondition.Snow)]
        [InlineData(701, WeatherCondition.Fog)]
        [InlineData(781, WeatherCondition.Fog)]
        [InlineData(800, WeatherCondition.Clear)]
        [InlineData(801, WeatherCondition.Clouds)]
        [InlineData(803, WeatherCondition.Clouds)]
        [InlineData(804, WeatherCondition.Overcast)]
        [InlineData(999, WeatherCondition.Unknown)]
        public void GetConditionFromCode_MapsOpenWeatherCodesCorrectly(int code, WeatherCondition expected)
        {
            var result = WeatherCodeMapper.GetConditionFromCode(code);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(200, "Weather/storm.png")]
        [InlineData(300, "Weather/rainy.png")]
        [InlineData(500, "Weather/rainy.png")]
        [InlineData(600, "Weather/rainy.png")]
        [InlineData(701, "Weather/overcast.png")]
        [InlineData(800, "Weather/sunny_clear.png")]
        [InlineData(802, "Weather/sunny_cloudy.png")]
        [InlineData(804, "Weather/overcast.png")]
        public void GetImageName_ReturnsExpectedAsset(int code, string expectedPath)
        {
            var result = WeatherCodeMapper.GetImageName(code);
            Assert.Equal(expectedPath, result);
        }
    }
}
