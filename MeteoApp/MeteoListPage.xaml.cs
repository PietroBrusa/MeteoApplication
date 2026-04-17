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
            // try/catch required in async void event handlers
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
            // Load data only on first appearance to avoid redundant API calls
            if (viewModel.Entries.Count == 0)
            {
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

    // Shows an ActionSheet to let the user pick Light / Dark / System theme
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

    // Toggles °C / °F and refreshes the list
    private async void OnToggleTemperatureUnit(object sender, EventArgs e)
    {
        if (BindingContext is MeteoListViewModel viewModel)
        {
            await viewModel.ToggleTemperatureUnitAsync();
        }
    }

    // Cycles through en → it → de and rebuilds the UI in the new language
    private void OnChangeLanguageClicked(object sender, EventArgs e)
    {
        var current = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        string newCulture = "en";

        if (current == "en") newCulture = "it";
        else if (current == "it") newCulture = "de";
        else if (current == "de") newCulture = "en";

        Console.WriteLine($"Language change: {current} → {newCulture}");
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

        // Clear selection so the same item can be tapped again
        if (sender is CollectionView cv)
        {
            cv.SelectedItem = null;
        }
    }
}
