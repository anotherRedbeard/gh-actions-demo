namespace WeatherApp.Web.Models;

public class WeatherViewModel
{
    public string? ZipCode { get; set; }
    public string? Location { get; set; }
    public string? ErrorMessage { get; set; }
    public WeatherForecast[] Forecasts { get; set; } = Array.Empty<WeatherForecast>();
}

public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
}

public class WeatherResponse
{
    public string ZipCode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public WeatherForecast[] Forecasts { get; set; } = Array.Empty<WeatherForecast>();
}
