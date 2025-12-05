using Microsoft.AspNetCore.Mvc;

namespace WeatherApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get weather forecast for a specific zip code
    /// </summary>
    /// <param name="zipCode">US zip code (5 digits)</param>
    /// <returns>Weather forecast for the next 5 days</returns>
    [HttpGet("{zipCode}")]
    public ActionResult<WeatherResponse> GetByZipCode(string zipCode)
    {
        // Validate zip code format (5 digits)
        if (string.IsNullOrWhiteSpace(zipCode) || zipCode.Length != 5 || !zipCode.All(char.IsDigit))
        {
            return BadRequest(new { error = "Invalid zip code. Please provide a 5-digit US zip code." });
        }

        // Generate mock weather data based on zip code
        // In a real application, this would call a weather service
        var random = new Random(zipCode.GetHashCode());
        var baseTemp = random.Next(-10, 40);

        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = baseTemp + random.Next(-5, 10),
            Summary = Summaries[random.Next(Summaries.Length)]
        }).ToArray();

        return Ok(new WeatherResponse
        {
            ZipCode = zipCode,
            Location = GetLocationName(zipCode),
            Forecasts = forecasts
        });
    }

    /// <summary>
    /// Get weather forecast (default - 5 day forecast)
    /// </summary>
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    private static string GetLocationName(string zipCode)
    {
        // Mock location names - in a real app this would lookup the zip code
        var locations = new Dictionary<string, string>
        {
            { "10001", "New York, NY" },
            { "90210", "Beverly Hills, CA" },
            { "60601", "Chicago, IL" },
            { "77001", "Houston, TX" },
            { "85001", "Phoenix, AZ" },
            { "19101", "Philadelphia, PA" },
            { "78201", "San Antonio, TX" },
            { "92101", "San Diego, CA" },
            { "75201", "Dallas, TX" },
            { "95101", "San Jose, CA" }
        };

        return locations.TryGetValue(zipCode, out var location) 
            ? location 
            : $"Location {zipCode}";
    }
}
