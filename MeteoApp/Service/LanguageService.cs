using System;
using System.Globalization;

namespace MeteoApp
{
    public class LanguageService
    {
        // Fired after the language changes so the UI can rebuild with the new culture
        public event Action? LanguageChanged;

        // Sets the app culture on all relevant threads and persists the choice
        public void SetLanguage(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            new SettingsService().SaveLanguage(cultureCode);

            LanguageChanged?.Invoke();
        }
    }
}
