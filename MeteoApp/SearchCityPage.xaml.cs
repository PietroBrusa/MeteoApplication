using Microsoft.Maui.Controls;

namespace MeteoApp;

public partial class SearchCityPage : ContentPage
{
    private MeteoListViewModel _viewModel;

    public SearchCityPage(MeteoListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSearchPressed(object sender, EventArgs e)
    {
        string query = CitySearchBar.Text;
        if (!string.IsNullOrWhiteSpace(query))
        {
            var results = await _viewModel.SearchCitiesAsync(query);

            if (results != null && results.Count > 0)
            {
                ResultsCollectionView.ItemsSource = results;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Attenzione", "Nessuna città trovata con questo nome. Riprova.", "OK");
                ResultsCollectionView.ItemsSource = null;
            }
        }
    }

    private async void OnCitySelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedCity = e.CurrentSelection?.FirstOrDefault() as MeteoLocation;

        if (selectedCity != null)
        {
            await _viewModel.AddCityAsync(selectedCity.Name);
            await Navigation.PopModalAsync();
        }

        if (sender is CollectionView cv)
        {
            cv.SelectedItem = null;
        }
    }
}