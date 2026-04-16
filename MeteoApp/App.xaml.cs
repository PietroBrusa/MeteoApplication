using System.Globalization;

namespace MeteoApp;

public partial class App : Application
{

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
        // Carica le preferenze salvate prima di costruire l'interfaccia (AS7)
        var settingsService = new SettingsService();
        settingsService.LoadTemperatureUnit();

        // Applica il tema salvato (AS6 + AS7)
        SettingsService.ApplyTheme(settingsService.LoadTheme());

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

        LanguageService.LanguageChanged += () =>
        {
            window.Page = new NavigationPage(new MeteoListPage());
        };

        return window;
    }
}