using MeteoApp.Resources.Strings;
using Microsoft.Maui.Controls;

namespace MeteoApp;

public partial class MeteoListPage : ContentPage
{
    public MeteoListPage()
    {
        InitializeComponent();
        BindingContext = new MeteoListViewModel();
    }

    private async void OnItemAdded(object sender, EventArgs e)
    {
        if (BindingContext is MeteoListViewModel viewModel)
        {
            await Navigation.PushModalAsync(new SearchCityPage(viewModel));
        }
    }

    private async void OnItemRemoved(object sender, EventArgs e)
    {
        var menuItem = sender as SwipeItem;
        var entryToDelete = menuItem?.CommandParameter as MeteoLocation;

        if (entryToDelete != null && BindingContext is MeteoListViewModel viewModel)
        {
            // try/catch obbligatorio negli async void handler (AS3)
            try
            {
                await viewModel.RemoveCityAsync(entryToDelete.Id);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, AppResources.OkButton);
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MeteoListViewModel viewModel)
        {
            if (viewModel.Entries.Count == 0)
            {
                // try/catch obbligatorio negli async void handler (AS3)
                try
                {
                    await viewModel.LoadLocationsFromDatabaseAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, AppResources.OkButton);
                }
            }
        }
    }

    // Mostra un ActionSheet per scegliere il tema (AS6 + AS7)
    private async void OnThemeClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet(
            AppResources.ThemeTitle,
            AppResources.CloseButton,
            null,
            AppResources.ThemeLight,
            AppResources.ThemeDark,
            AppResources.ThemeSystem);

        if (action == null || action == AppResources.CloseButton) return;

        string themeKey = action switch
        {
            var s when s == AppResources.ThemeLight  => "Light",
            var s when s == AppResources.ThemeDark   => "Dark",
            _                                        => "System"
        };

        new SettingsService().SaveTheme(themeKey);
        SettingsService.ApplyTheme(themeKey);
    }

    // Alterna tra °C e °F e ricarica la lista (AS7)
    private async void OnToggleTemperatureUnit(object sender, EventArgs e)
    {
        if (BindingContext is MeteoListViewModel viewModel)
        {
            await viewModel.ToggleTemperatureUnitAsync();
        }
    }

    private void OnChangeLanguageClicked(object sender, EventArgs e)
    {
        var current = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        string newCulture = "en";

        if (current == "en") newCulture = "it";
        else if (current == "it") newCulture = "de";
        else if (current == "de") newCulture = "en";

        Console.WriteLine($"Cambio lingua da {current} a {newCulture}");
        App.LanguageService.SetLanguage(newCulture);
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection?.FirstOrDefault();
        if (selected is MeteoLocation meteoLocation)
        {
            await Navigation.PushAsync(new MeteoItemPage
            {
                MeteoLocation = meteoLocation
            });
        }

        if (sender is CollectionView cv)
        {
            cv.SelectedItem = null;
        }
    }
}