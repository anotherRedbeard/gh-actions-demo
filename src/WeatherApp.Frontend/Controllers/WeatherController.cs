using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WeatherApp.Frontend.Models;

namespace WeatherApp.Frontend.Controllers;

public class WeatherController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<WeatherController> logger)
    {
        _httpClient = httpClientFactory.CreateClient("WeatherApi");
        _configuration = configuration;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(new WeatherViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> GetWeather(string zipCode)
    {
        var viewModel = new WeatherViewModel { ZipCode = zipCode };

        if (string.IsNullOrWhiteSpace(zipCode))
        {
            viewModel.ErrorMessage = "Please enter a zip code.";
            return View("Index", viewModel);
        }

        // Validate zip code format
        if (zipCode.Length != 5 || !zipCode.All(char.IsDigit))
        {
            viewModel.ErrorMessage = "Please enter a valid 5-digit US zip code.";
            return View("Index", viewModel);
        }

        try
        {
            var apiBaseUrl = _configuration["WeatherApi:BaseUrl"] ?? "http://localhost:7071";
            var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/weather/{zipCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                viewModel.Weather = JsonSerializer.Deserialize<WeatherData>(content, options);
            }
            else
            {
                viewModel.ErrorMessage = "Unable to retrieve weather data. Please try again later.";
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling weather API for zip code: {ZipCode}", zipCode);
            viewModel.ErrorMessage = "Weather service is currently unavailable. Please try again later.";
        }

        return View("Index", viewModel);
    }
}
