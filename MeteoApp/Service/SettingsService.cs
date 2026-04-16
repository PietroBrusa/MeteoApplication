namespace MeteoApp;

public class SettingsService
{
    const string LanguageKey = "app_language";
    const string TemperatureUnitKey = "temperature_unit";
    const string ThemeKey = "app_theme";

    // Stato corrente dell'unità, accessibile staticamente per le proprietà di display dei modelli
    public static string CurrentUnit { get; private set; } = "C";

    // --- Lingua ---

    public string LoadLanguage()
        => Preferences.Get(LanguageKey, string.Empty);

    public void SaveLanguage(string cultureCode)
        => Preferences.Set(LanguageKey, cultureCode);

    // --- Unità di temperatura ---

    public void LoadTemperatureUnit()
    {
        CurrentUnit = Preferences.Get(TemperatureUnitKey, "C");
    }

    public void SaveTemperatureUnit(string unit)
    {
        CurrentUnit = unit;
        Preferences.Set(TemperatureUnitKey, unit);
    }

    // --- Tema (AS6 + AS7) ---

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

    // --- Formattazione ---

    public static string FormatTemperature(double celsius)
    {
        if (CurrentUnit == "F")
            return $"{celsius * 9.0 / 5.0 + 32:F1}°F";
        return $"{celsius:F1}°C";
    }
}
