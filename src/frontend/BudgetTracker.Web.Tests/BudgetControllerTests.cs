using System.Net;
using System.Text.Json;
using BudgetTracker.Web.Controllers;
using BudgetTracker.Web.Models;
using BudgetTracker.Web.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using Xunit;

namespace BudgetTracker.Web.Tests;

public class BudgetControllerTests
{
    private readonly BudgetController _sut;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly BudgetApiClient _apiClient;

    public BudgetControllerTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:7071/api/")
        };
        _apiClient = new BudgetApiClient(httpClient);
        _sut = new BudgetController(_apiClient);
    }

    // ── Index ───────────────────────────────────────────────────

    [Fact]
    public async Task Index_ShouldReturnViewResult_WithListOfBudgets()
    {
        // Arrange
        var budgets = new List<Budget>
        {
            new() { Name = "January Budget", TotalAmount = 3000 },
            new() { Name = "February Budget", TotalAmount = 4000 }
        };
        SetupGetResponse("budgets", budgets);

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<List<Budget>>().Subject;
        model.Should().HaveCount(2);
    }

    [Fact]
    public async Task Index_WhenApiReturnsNull_ShouldReturnEmptyList()
    {
        // Arrange
        SetupGetResponse<List<Budget>?>("budgets", null);

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<List<Budget>>().Subject;
        model.Should().BeEmpty();
    }

    // ── Details ─────────────────────────────────────────────────

    [Fact]
    public async Task Details_WithValidId_ShouldReturnViewResult()
    {
        // Arrange
        var budget = new Budget { Name = "Test Budget", TotalAmount = 2500 };
        SetupGetResponse($"budgets/{budget.Id}", budget);

        // Act
        var result = await _sut.Details(budget.Id);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<Budget>().Subject;
        model.Name.Should().Be("Test Budget");
    }

    [Fact]
    public async Task Details_WhenApiReturnsNull_ShouldReturnNotFound()
    {
        // Arrange
        SetupGetResponse<Budget?>("budgets/nonexistent", null);

        // Act
        var result = await _sut.Details("nonexistent");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    // ── Create (GET) ────────────────────────────────────────────

    [Fact]
    public void Create_Get_ShouldReturnViewResult()
    {
        // Arrange & Act
        var result = _sut.Create();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    // ── Create (POST) ───────────────────────────────────────────

    [Fact]
    public async Task Create_Post_WithValidModel_ShouldRedirectToIndex()
    {
        // Arrange
        var budget = new Budget { Name = "New Budget", TotalAmount = 5000 };
        SetupPostResponse("budgets", budget);

        // Act
        var result = await _sut.Create(budget);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Create_Post_WithInvalidModelState_ShouldReturnView()
    {
        // Arrange
        var budget = new Budget { Name = "", TotalAmount = 0 };
        _sut.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _sut.Create(budget);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<Budget>();
    }

    // ── Helpers ─────────────────────────────────────────────────

    private void SetupGetResponse<T>(string endpoint, T responseBody)
    {
        var json = JsonSerializer.Serialize(responseBody);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

        _httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get &&
                    r.RequestUri!.ToString().Contains(endpoint)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void SetupPostResponse<T>(string endpoint, T responseBody)
    {
        var json = JsonSerializer.Serialize(responseBody);
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

        _httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post &&
                    r.RequestUri!.ToString().Contains(endpoint)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }
}
