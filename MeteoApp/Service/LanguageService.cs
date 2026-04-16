using System;
using System.Globalization;

namespace MeteoApp
{
    public class LanguageService
    {
        public event Action? LanguageChanged;

        public void SetLanguage(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Salva la lingua scelta in modo che persista al riavvio (AS7)
            new SettingsService().SaveLanguage(cultureCode);

            LanguageChanged?.Invoke();
        }
    }
}