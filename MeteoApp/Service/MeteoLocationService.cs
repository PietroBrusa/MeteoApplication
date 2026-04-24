using MeteoApp;
using System;
using System.Collections.Generic;
using System.Text;

public class MeteoLocationService
{
    public MeteoLocation? CurrentLocation { get; set; }

    public event Action? OnLocationChanged;

    public void SetLocation(MeteoLocation location)
    {
        CurrentLocation = location;
        OnLocationChanged?.Invoke();
    }
}
