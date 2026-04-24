using System;
using System.Collections.Generic;
using System.Text;

namespace MeteoApp;

public partial class MeteoDetailPage : ContentPage
{
    public MeteoLocation Location { get; }
    public string Title => Location?.Name ?? "Details";

    public MeteoDetailPage(MeteoLocation location)
    {
        InitializeComponent();
        Location = location;
        BindingContext = this;

        var locationService = IPlatformApplication.Current?.Services
            .GetService<MeteoLocationService>();

        if (locationService != null)
            locationService.SetLocation(location);
    }
}
