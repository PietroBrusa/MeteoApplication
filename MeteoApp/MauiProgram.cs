using Microsoft.Extensions.Logging;

namespace MeteoApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiMaps()   // Required for the Map control used in SearchCityPage and MeteoItemPage
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<ParameterService>();
		builder.Services.AddMauiBlazorWebView();
		

#if DEBUG
        builder.Logging.AddDebug();
#endif
		return builder.Build();
	}
}
