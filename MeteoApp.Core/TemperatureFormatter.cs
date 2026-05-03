namespace MeteoApp
{
    public static class TemperatureFormatter
    {
        public static string Format(double celsius, string unit)
        {
            if (unit == "F")
                return $"{celsius * 9.0 / 5.0 + 32:F1}°F";
            return $"{celsius:F1}°C";
        }
    }
}
