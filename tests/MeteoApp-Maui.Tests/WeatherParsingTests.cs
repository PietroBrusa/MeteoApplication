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
                    ""pressure"": 1012,
                    ""humidity"": 81
                },
                ""weather"": [
                    { 
                        ""id"": 300, 
                        ""main"": ""Drizzle"", 
                        ""description"": ""light intensity drizzle"" 
                    }
                ]
            }";

            var result = JsonConvert.DeserializeObject<WeatherResponse>(mockJson);

            Assert.NotNull(result);
            Assert.Equal("London", result.name);
            Assert.Equal(17.36, result.main.temp);
            Assert.NotNull(result.weather);
            Assert.NotEmpty(result.weather);
            Assert.Equal("light intensity drizzle", result.weather[0].description);
        }
    }
}