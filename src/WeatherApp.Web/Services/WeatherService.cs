using WeatherApp.Web.Models;

namespace WeatherApp.Web.Services;

public interface IWeatherService
{
    Task<WeatherResponse> GetWeatherByZipCodeAsync(string zipCode);
}

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;
    private readonly string _apiBaseUrl;

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiBaseUrl = configuration["WeatherApi:BaseUrl"] ?? "https://localhost:7001";
    }

    public async Task<WeatherResponse> GetWeatherByZipCodeAsync(string zipCode)
    {
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/weatherforecast/{zipCode}");
        response.EnsureSuccessStatusCode();
        
        var weather = await response.Content.ReadFromJsonAsync<WeatherResponse>();
        return weather ?? throw new InvalidOperationException("Failed to deserialize weather response");
    }
}
