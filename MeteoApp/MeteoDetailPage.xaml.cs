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

        // Pass location data to Blazor via a shared service
        var locationService = Handler?.MauiContext?.Services
            .GetService<MeteoLocationService>();

        if (locationService != null)
            locationService.CurrentLocation = location;
    }
}
