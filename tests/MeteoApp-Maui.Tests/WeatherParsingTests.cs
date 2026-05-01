using MeteoApp;
using Newtonsoft.Json;

namespace MeteoApp.Tests
{
    public class WeatherParsingTests
    {
        [Fact]
        public void WeatherResponse_Deserializes_Correctly()
        {
            string mockJson = @"{
                ""name"": ""London"",
                ""main"": {
                    ""temp"": 17.36,
                    ""temp_min"": 15.0,
                    ""temp_max"": 20.0,
                    ""pressure"": 1012,
                    ""humidity"": 81
                },
                ""weather"": [
                    {
                        ""id"": 300,
                        ""main"": ""Drizzle"",
                        ""description"": ""light intensity drizzle""
                    }
                ],
                ""coord"": { ""lat"": 51.51, ""lon"": -0.13 }
            }";

            var result = JsonConvert.DeserializeObject<WeatherResponse>(mockJson);

            Assert.NotNull(result);
            Assert.Equal("London", result!.name);
            Assert.Equal(17.36, result.main.temp);
            Assert.Equal(15.0, result.main.temp_min);
            Assert.Equal(20.0, result.main.temp_max);
            Assert.NotNull(result.weather);
            Assert.NotEmpty(result.weather);
            Assert.Equal("light intensity drizzle", result.weather[0].description);
            Assert.Equal(51.51, result.coord.lat);
        }

        [Fact]
        public void WeatherResponse_WithEmptyWeatherList_ReturnsEmptyList()
        {
            string mockJson = @"{
                ""name"": ""Test"",
                ""main"": { ""temp"": 10.0, ""temp_min"": 8.0, ""temp_max"": 12.0 },
                ""weather"": [],
                ""coord"": { ""lat"": 0.0, ""lon"": 0.0 }
            }";

            var result = JsonConvert.DeserializeObject<WeatherResponse>(mockJson);

            Assert.NotNull(result);
            Assert.Empty(result!.weather);
        }

        [Fact]
        public void FormatTemperature_Celsius_ReturnsCorrectFormat()
        {
            string result = TemperatureFormatter.Format(17.36, "C");
            Assert.Equal("17.4°C", result);
        }

        [Fact]
        public void FormatTemperature_Fahrenheit_ConvertsCorrectly()
        {
            string result = TemperatureFormatter.Format(0.0, "F");
            Assert.Equal("32.0°F", result);
        }

        [Fact]
        public void FormatTemperature_UnknownUnit_FallsBackToCelsius()
        {
            string result = TemperatureFormatter.Format(20.0, "X");
            Assert.Equal("20.0°C", result);
        }

        [Fact]
        public async Task WeatherFindResponse_Deserializes_ListCorrectly()
        {
            string mockJson = @"{
                ""list"": [
                    {
                        ""name"": ""London"",
                        ""main"": { ""temp"": 17.0, ""temp_min"": 15.0, ""temp_max"": 19.0 },
                        ""weather"": [{ ""id"": 300, ""main"": ""Drizzle"", ""description"": ""drizzle"" }],
                        ""coord"": { ""lat"": 51.51, ""lon"": -0.13 },
                        ""sys"": { ""country"": ""GB"" }
                    },
                    {
                        ""name"": ""Londonderry"",
                        ""main"": { ""temp"": 14.0, ""temp_min"": 12.0, ""temp_max"": 16.0 },
                        ""weather"": [{ ""id"": 800, ""main"": ""Clear"", ""description"": ""clear sky"" }],
                        ""coord"": { ""lat"": 54.99, ""lon"": -7.32 },
                        ""sys"": { ""country"": ""GB"" }
                    }
                ]
            }";

            var result = await Task.Run(() =>
                JsonConvert.DeserializeObject<WeatherFindResponse>(mockJson));

            Assert.NotNull(result);
            Assert.Equal(2, result!.list.Count);
            Assert.Equal("London", result.list[0].name);
            Assert.Equal("Londonderry", result.list[1].name);
        }
    }
}
