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

            LanguageChanged?.Invoke();
        }
    }
}