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
        InitializeComponent();

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;

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