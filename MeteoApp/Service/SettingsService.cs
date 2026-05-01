namespace MeteoApp;

public class SettingsService
{
    const string LanguageKey = "app_language";
    const string TemperatureUnitKey = "temperature_unit";
    const string ThemeKey = "app_theme";

    public static string CurrentUnit { get; private set; } = "C";

    public string LoadLanguage()
        => Preferences.Get(LanguageKey, string.Empty);

    public void SaveLanguage(string cultureCode)
        => Preferences.Set(LanguageKey, cultureCode);

    public void LoadTemperatureUnit()
    {
        CurrentUnit = Preferences.Get(TemperatureUnitKey, "C");
    }

    public void SaveTemperatureUnit(string unit)
    {
        CurrentUnit = unit;
        Preferences.Set(TemperatureUnitKey, unit);
    }

    public string LoadTheme()
        => Preferences.Get(ThemeKey, "System");

    public void SaveTheme(string theme)
        => Preferences.Set(ThemeKey, theme);

    public static void ApplyTheme(string theme)
    {
        Application.Current.UserAppTheme = theme switch
        {
            "Dark"  => AppTheme.Dark,
            "Light" => AppTheme.Light,
            _       => AppTheme.Unspecified
        };
    }

    public static string FormatTemperature(double celsius)
        => TemperatureFormatter.Format(celsius, CurrentUnit);
}
