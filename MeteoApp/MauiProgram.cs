using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
#if ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif

namespace MeteoApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiMaps()   // Required for the Map control used in SearchCityPage and MeteoItemPage
			.RegisterFirebaseServices()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Services.AddSingleton<MeteoLocationService>();
        builder.Services.AddSingleton<WeatherApiService>();
        builder.Services.AddSingleton<NotificationService>();
        builder.Services.AddSingleton<AppwriteSyncService>();
        builder.Services.AddMauiBlazorWebView();


#if DEBUG
        builder.Logging.AddDebug();
#endif
		return builder.Build();
	}

	// Initializes Plugin.Firebase via the MAUI lifecycle (slide 6.1 + plugin v4 README).
	// FirebaseOptions/name are omitted: the plugin reads them from google-services.json (Platforms/Android).
	private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
	{
		builder.ConfigureLifecycleEvents(events =>
		{
#if ANDROID
			events.AddAndroid(android => android.OnCreate((activity, _) =>
				CrossFirebase.Initialize(
					activity,
					() => Microsoft.Maui.ApplicationModel.Platform.CurrentActivity!)));
#endif
		});
		return builder;
	}
}
