namespace WeatherApp.Api;

public class WeatherResponse
{
    public string ZipCode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public WeatherForecast[] Forecasts { get; set; } = Array.Empty<WeatherForecast>();
}
