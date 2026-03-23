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

public class SavingsGoalControllerTests
{
    private readonly SavingsGoalController _sut;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly BudgetApiClient _apiClient;

    public SavingsGoalControllerTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:7071/api/")
        };
        _apiClient = new BudgetApiClient(httpClient);
        _sut = new SavingsGoalController(_apiClient);
    }

    // ── Index ───────────────────────────────────────────────────

    [Fact]
    public async Task Index_ShouldReturnViewResult_WithListOfGoals()
    {
        // Arrange
        var goals = new List<SavingsGoal>
        {
            new() { Name = "Emergency Fund", TargetAmount = 10000, CurrentAmount = 5000 },
            new() { Name = "Vacation", TargetAmount = 3000, CurrentAmount = 1500 }
        };
        SetupGetResponse("savings-goals", goals);

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<List<SavingsGoal>>().Subject;
        model.Should().HaveCount(2);
    }

    [Fact]
    public async Task Index_WhenApiReturnsNull_ShouldReturnEmptyList()
    {
        // Arrange
        SetupGetResponse<List<SavingsGoal>?>("savings-goals", null);

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<List<SavingsGoal>>().Subject;
        model.Should().BeEmpty();
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
        var goal = new SavingsGoal
        {
            Name = "New Car",
            TargetAmount = 25000,
            CurrentAmount = 0,
            TargetDate = DateTime.UtcNow.AddYears(2),
            MonthlyContribution = 1000
        };
        SetupPostResponse("savings-goals", goal);

        // Act
        var result = await _sut.Create(goal);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Create_Post_WithInvalidModelState_ShouldReturnView()
    {
        // Arrange
        var goal = new SavingsGoal { Name = "" };
        _sut.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _sut.Create(goal);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<SavingsGoal>();
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
