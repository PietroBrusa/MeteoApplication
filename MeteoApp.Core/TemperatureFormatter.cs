namespace MeteoApp
{
    public static class TemperatureFormatter
    {
        /// <summary>
        /// Formats a Celsius value as a display string in the requested unit.
        /// </summary>
        /// <param name="unit">"C" or "F". Any other value falls back to Celsius.</param>
        public static string Format(double celsius, string unit)
        {
            if (unit == "F")
                return $"{celsius * 9.0 / 5.0 + 32:F1}°F";
            return $"{celsius:F1}°C";
        }
    }
}
