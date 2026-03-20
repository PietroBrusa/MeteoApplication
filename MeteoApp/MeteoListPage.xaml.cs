using Android.Content.Res;
using MeteoApp.Resources.Strings;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using static Android.Security.Identity.CredentialDataResult;

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
        string cityName = await DisplayPromptAsync(
            AppResources.AddCityTitle,
            AppResources.AddCityMessage);

        if (!string.IsNullOrWhiteSpace(cityName))
        {
            if (BindingContext is MeteoListViewModel viewModel)
            {
                await viewModel.AddCityAsync(cityName);
            }
        }
    }

    private async void OnItemRemoved(object sender, EventArgs e)
    {
        var menuItem = sender as SwipeItem;
        var entryToDelete = menuItem?.CommandParameter as MeteoLocation;

        if (entryToDelete != null)
        {
            if (BindingContext is MeteoListViewModel viewModel)
            {
                await viewModel.RemoveCityAsync(entryToDelete.Id);
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MeteoListViewModel viewModel)
        {
            await viewModel.LoadLocationsFromDatabaseAsync();
        }
    }

    private void OnChangeLanguageClicked(object sender, EventArgs e)
    {
        var current = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        string newCulture = "en"; 

        if (current == "en")
            newCulture = "it";
        else if (current == "it")
            newCulture = "de";
        else if (current == "de")
            newCulture = "en";

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