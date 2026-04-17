using System.Globalization;

namespace MeteoApp;

public partial class App : Application
{
    // Singleton database instance shared across the app
    private static MeteoDatabase database;

    public static MeteoDatabase Database
    {
        get
        {
            if (database == null)
            {
                database = new MeteoDatabase();
            }
            return database;
        }
    }

    public static readonly LanguageService LanguageService = new();

    public App()
    {
        // Load persisted settings before building the UI
        var settingsService = new SettingsService();
        settingsService.LoadTemperatureUnit();
        SettingsService.ApplyTheme(settingsService.LoadTheme());

        // Restore the saved language, or keep the system default
        var savedLanguage = settingsService.LoadLanguage();
        if (!string.IsNullOrEmpty(savedLanguage))
        {
            var culture = new CultureInfo(savedLanguage);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
        else
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;
        }

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new NavigationPage(new MeteoListPage()));

        // Rebuild the page tree when the language changes so resource strings refresh
        LanguageService.LanguageChanged += () =>
        {
            window.Page = new NavigationPage(new MeteoListPage());
        };

        return window;
    }
}
