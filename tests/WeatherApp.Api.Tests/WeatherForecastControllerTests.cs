using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherApp.Api.Controllers;
using Xunit;

namespace WeatherApp.Api.Tests;

public class WeatherForecastControllerTests
{
    private readonly WeatherForecastController _controller;
    private readonly Mock<ILogger<WeatherForecastController>> _loggerMock;

    public WeatherForecastControllerTests()
    {
        _loggerMock = new Mock<ILogger<WeatherForecastController>>();
        _controller = new WeatherForecastController(_loggerMock.Object);
    }

    [Fact]
    public void Get_ReturnsForecasts()
    {
        // Act
        var result = _controller.Get();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
    }

    [Fact]
    public void GetByZipCode_ValidZipCode_ReturnsWeatherResponse()
    {
        // Arrange
        var zipCode = "10001";

        // Act
        var result = _controller.GetByZipCode(zipCode);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<WeatherResponse>(okResult.Value);
        Assert.Equal(zipCode, response.ZipCode);
        Assert.Equal("New York, NY", response.Location);
        Assert.Equal(5, response.Forecasts.Length);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("1234567")]
    [InlineData("abcde")]
    [InlineData(null)]
    public void GetByZipCode_InvalidZipCode_ReturnsBadRequest(string zipCode)
    {
        // Act
        var result = _controller.GetByZipCode(zipCode);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void GetByZipCode_UnknownZipCode_ReturnsGenericLocation()
    {
        // Arrange
        var zipCode = "99999";

        // Act
        var result = _controller.GetByZipCode(zipCode);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<WeatherResponse>(okResult.Value);
        Assert.Equal("Location 99999", response.Location);
    }
}
