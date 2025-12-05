using Microsoft.AspNetCore.Mvc;
using WeatherApp.Web.Models;
using WeatherApp.Web.Services;

namespace WeatherApp.Web.Controllers;

public class WeatherController : Controller
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(new WeatherViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Search(string zipCode)
    {
        var viewModel = new WeatherViewModel { ZipCode = zipCode };

        if (string.IsNullOrWhiteSpace(zipCode) || zipCode.Length != 5 || !zipCode.All(char.IsDigit))
        {
            viewModel.ErrorMessage = "Please enter a valid 5-digit US zip code.";
            return View("Index", viewModel);
        }

        try
        {
            var weather = await _weatherService.GetWeatherByZipCodeAsync(zipCode);
            viewModel.Location = weather.Location;
            viewModel.Forecasts = weather.Forecasts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather for zip code: {ZipCode}", zipCode);
            viewModel.ErrorMessage = "Unable to fetch weather data. Please try again later.";
        }

        return View("Index", viewModel);
    }
}
