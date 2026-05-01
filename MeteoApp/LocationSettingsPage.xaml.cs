using System.ComponentModel;
using System.Runtime.CompilerServices;
using MeteoApp.Resources.Strings;

namespace MeteoApp;

public partial class LocationSettingsPage : ContentPage, INotifyPropertyChanged
{
    public MeteoLocation MeteoLocation { get; }

    public bool NotificationsEnabled
    {
        get => MeteoLocation.NotificationsEnabled;
        set
        {
            if (MeteoLocation.NotificationsEnabled == value) return;
            MeteoLocation.NotificationsEnabled = value;
            OnPropertyChanged();
        }
    }

    public double TempThresholdMin
    {
        get => MeteoLocation.TempThresholdMin;
        set
        {
            if (MeteoLocation.TempThresholdMin == value) return;
            MeteoLocation.TempThresholdMin = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TempThresholdMinDisplay));
        }
    }

    public double TempThresholdMax
    {
        get => MeteoLocation.TempThresholdMax;
        set
        {
            if (MeteoLocation.TempThresholdMax == value) return;
            MeteoLocation.TempThresholdMax = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TempThresholdMaxDisplay));
        }
    }

    public string TempThresholdMinDisplay => SettingsService.FormatTemperature(MeteoLocation.TempThresholdMin);
    public string TempThresholdMaxDisplay => SettingsService.FormatTemperature(MeteoLocation.TempThresholdMax);

    public LocationSettingsPage(MeteoLocation location)
    {
        InitializeComponent();
        MeteoLocation = location;
        BindingContext = this;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            await App.Database.UpdateLocationAsync(MeteoLocation);

            var services = IPlatformApplication.Current?.Services;
            var sync = services?.GetService<AppwriteSyncService>();
            var notify = services?.GetService<NotificationService>();
            if (sync != null && notify != null)
            {
                var token = notify.GetCachedToken();
                var docId = await sync.SyncLocationAsync(MeteoLocation, token);
                if (!string.IsNullOrEmpty(docId) && docId != MeteoLocation.AppwriteDocumentId)
                {
                    MeteoLocation.AppwriteDocumentId = docId;
                    await App.Database.UpdateLocationAsync(MeteoLocation);
                }
            }

            await DisplayAlert(AppResources.NotificationsTitle, AppResources.SettingsSavedMessage, AppResources.OkButton);
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, AppResources.OkButton);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
