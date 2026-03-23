using System.Diagnostics;
using BudgetTracker.Web.Controllers;
using BudgetTracker.Web.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BudgetTracker.Web.Tests;

public class HomeControllerTests
{
    private readonly HomeController _sut;
    private readonly Mock<ILogger<HomeController>> _loggerMock;

    public HomeControllerTests()
    {
        _loggerMock = new Mock<ILogger<HomeController>>();
        _sut = new HomeController(_loggerMock.Object);
    }

    // ── Index ───────────────────────────────────────────────────

    [Fact]
    public void Index_ShouldReturnViewResult()
    {
        // Arrange & Act
        var result = _sut.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    // ── Privacy ─────────────────────────────────────────────────

    [Fact]
    public void Privacy_ShouldReturnViewResult()
    {
        // Arrange & Act
        var result = _sut.Privacy();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    // ── Error ───────────────────────────────────────────────────

    [Fact]
    public void Error_ShouldReturnViewResult_WithErrorViewModel()
    {
        // Arrange
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = _sut.Error();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
        model.RequestId.Should().NotBeNullOrEmpty();
    }
}
