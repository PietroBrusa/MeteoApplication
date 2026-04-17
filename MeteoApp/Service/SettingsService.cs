namespace MeteoApp;

public class SettingsService
{
    const string LanguageKey = "app_language";
    const string TemperatureUnitKey = "temperature_unit";
    const string ThemeKey = "app_theme";

    // Static so MeteoLocation computed props can read it without dependency injection
    public static string CurrentUnit { get; private set; } = "C";

    // --- Language ---

    public string LoadLanguage()
        => Preferences.Get(LanguageKey, string.Empty);

    public void SaveLanguage(string cultureCode)
        => Preferences.Set(LanguageKey, cultureCode);

    // --- Temperature unit ---

    public void LoadTemperatureUnit()
    {
        CurrentUnit = Preferences.Get(TemperatureUnitKey, "C");
    }

    public void SaveTemperatureUnit(string unit)
    {
        CurrentUnit = unit;
        Preferences.Set(TemperatureUnitKey, unit);
    }

    // --- Theme ---

    public string LoadTheme()
        => Preferences.Get(ThemeKey, "System");

    public void SaveTheme(string theme)
        => Preferences.Set(ThemeKey, theme);

    // Applies the chosen theme immediately to the running app
    public static void ApplyTheme(string theme)
    {
        Application.Current.UserAppTheme = theme switch
        {
            "Dark"  => AppTheme.Dark,
            "Light" => AppTheme.Light,
            _       => AppTheme.Unspecified
        };
    }

    // --- Formatting ---

    // Converts Celsius to the current unit and returns a display string
    public static string FormatTemperature(double celsius)
    {
        if (CurrentUnit == "F")
            return $"{celsius * 9.0 / 5.0 + 32:F1}°F";
        return $"{celsius:F1}°C";
    }
}
