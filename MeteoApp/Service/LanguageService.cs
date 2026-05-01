using System;
using System.Globalization;

namespace MeteoApp
{
    public class LanguageService
    {
        /// <summary>Fired after the language changes so the UI can rebuild with the new culture.</summary>
        public event Action? LanguageChanged;

        /// <summary>Sets the app culture on all relevant threads and persists the choice.</summary>
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
