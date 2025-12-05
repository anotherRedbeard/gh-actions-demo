namespace WeatherApp.Frontend.Models;

public class WeatherViewModel
{
    public string? ZipCode { get; set; }
    public WeatherData? Weather { get; set; }
    public string? ErrorMessage { get; set; }
}

public class WeatherData
{
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int Temperature { get; set; }
    public int FeelsLike { get; set; }
    public int Humidity { get; set; }
    public int WindSpeed { get; set; }
    public string Condition { get; set; } = string.Empty;
    public int High { get; set; }
    public int Low { get; set; }
    public DateTime LastUpdated { get; set; }
}
