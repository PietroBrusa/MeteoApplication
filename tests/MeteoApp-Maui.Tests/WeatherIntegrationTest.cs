using Xunit;
using MeteoApp;
using System.Threading.Tasks;

namespace MeteoApp.Tests
{
    public class WeatherIntegrationTests
    {
        [Fact]
        public async Task FetchWeatherForCityAsync_ReturnsValidDataFromApi()
        {
            var viewModel = new MeteoListViewModel();
            string testCity = "London";

            var result = await viewModel.FetchWeatherForCityAsync(testCity, 1);

            Assert.NotNull(result);
            Assert.Equal("London", result.Name);
            Assert.NotEqual(0, result.CurrentTemperature);
        }
    }
}