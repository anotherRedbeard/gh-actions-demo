using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace WeatherApp.Api.Functions;

public class WeatherFunction
{
    private readonly ILogger<WeatherFunction> _logger;

    public WeatherFunction(ILogger<WeatherFunction> logger)
    {
        _logger = logger;
    }

    [Function("GetWeather")]
    public async Task<HttpResponseData> GetWeather(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather/{zipCode}")] HttpRequestData req,
        string zipCode)
    {
        _logger.LogInformation("Getting weather for zip code: {ZipCode}", zipCode);

        // Validate zip code format (US zip codes are 5 digits)
        if (string.IsNullOrEmpty(zipCode) || !IsValidZipCode(zipCode))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Invalid zip code format. Please enter a 5-digit US zip code." });
            return badResponse;
        }

        // Generate mock weather data for demonstration
        var weatherData = GenerateMockWeather(zipCode);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(weatherData);
        return response;
    }

    private static bool IsValidZipCode(string zipCode)
    {
        return zipCode.Length == 5 && zipCode.All(char.IsDigit);
    }

    private static WeatherResponse GenerateMockWeather(string zipCode)
    {
        // Use zip code to generate consistent but varied mock data
        var seed = int.Parse(zipCode);
        var random = new Random(seed);

        var conditions = new[] { "Sunny", "Partly Cloudy", "Cloudy", "Rainy", "Stormy", "Snowy", "Foggy", "Clear" };
        var cities = GetMockCityForZip(zipCode);

        return new WeatherResponse
        {
            ZipCode = zipCode,
            City = cities,
            Temperature = random.Next(20, 95),
            FeelsLike = random.Next(18, 98),
            Humidity = random.Next(20, 90),
            WindSpeed = random.Next(0, 30),
            Condition = conditions[random.Next(conditions.Length)],
            High = random.Next(70, 100),
            Low = random.Next(30, 65),
            LastUpdated = DateTime.UtcNow
        };
    }

    private static string GetMockCityForZip(string zipCode)
    {
        // Some common zip code mappings for demonstration
        return zipCode switch
        {
            "10001" => "New York, NY",
            "90210" => "Beverly Hills, CA",
            "60601" => "Chicago, IL",
            "77001" => "Houston, TX",
            "85001" => "Phoenix, AZ",
            "19101" => "Philadelphia, PA",
            "78201" => "San Antonio, TX",
            "92101" => "San Diego, CA",
            "75201" => "Dallas, TX",
            "95101" => "San Jose, CA",
            "32801" => "Orlando, FL",
            "33101" => "Miami, FL",
            "98101" => "Seattle, WA",
            "80201" => "Denver, CO",
            "02101" => "Boston, MA",
            _ => $"City {zipCode}, US"
        };
    }
}

public class WeatherResponse
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
