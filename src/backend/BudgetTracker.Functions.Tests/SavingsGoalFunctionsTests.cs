using System.Text;
using System.Text.Json;
using BudgetTracker.Functions.Models;
using BudgetTracker.Functions.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BudgetTracker.Functions.Tests;

public class SavingsGoalFunctionsTests
{
    private readonly SavingsGoalFunctions _sut;
    private readonly DataService _dataService;
    private readonly Mock<ILogger<SavingsGoalFunctions>> _loggerMock;

    public SavingsGoalFunctionsTests()
    {
        _loggerMock = new Mock<ILogger<SavingsGoalFunctions>>();
        _dataService = new DataService();
        _sut = new SavingsGoalFunctions(_loggerMock.Object, _dataService);
    }

    // ── GetSavingsGoals ─────────────────────────────────────────

    [Fact]
    public void GetSavingsGoals_ShouldReturnOkResult_WithListOfGoals()
    {
        // Arrange
        var request = CreateGetRequest();

        // Act
        var result = _sut.GetSavingsGoals(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var goals = okResult.Value.Should().BeAssignableTo<List<SavingsGoal>>().Subject;
        goals.Should().NotBeEmpty();
    }

    [Fact]
    public void GetSavingsGoals_ShouldSetCorsHeaders()
    {
        // Arrange
        var request = CreateGetRequest();

        // Act
        _sut.GetSavingsGoals(request);

        // Assert
        request.HttpContext.Response.Headers["Access-Control-Allow-Origin"].ToString().Should().Be("*");
    }

    // ── GetSavingsGoal ──────────────────────────────────────────

    [Fact]
    public void GetSavingsGoal_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var existingGoal = _dataService.GetSavingsGoals().First();
        var request = CreateGetRequest();

        // Act
        var result = _sut.GetSavingsGoal(request, existingGoal.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var goal = okResult.Value.Should().BeOfType<SavingsGoal>().Subject;
        goal.Id.Should().Be(existingGoal.Id);
        goal.Name.Should().Be(existingGoal.Name);
    }

    [Fact]
    public void GetSavingsGoal_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var request = CreateGetRequest();

        // Act
        var result = _sut.GetSavingsGoal(request, "nonexistent-id");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void GetSavingsGoal_ShouldSetCorsHeaders()
    {
        // Arrange
        var existingGoal = _dataService.GetSavingsGoals().First();
        var request = CreateGetRequest();

        // Act
        _sut.GetSavingsGoal(request, existingGoal.Id);

        // Assert
        request.HttpContext.Response.Headers["Access-Control-Allow-Origin"].ToString().Should().Be("*");
    }

    // ── CreateSavingsGoal ───────────────────────────────────────

    [Fact]
    public async Task CreateSavingsGoal_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        var newGoal = new SavingsGoal
        {
            Name = "New Car Fund",
            TargetAmount = 30000,
            CurrentAmount = 5000,
            TargetDate = DateTime.UtcNow.AddYears(2),
            MonthlyContribution = 1000,
            Color = "#3B82F6"
        };
        var request = CreatePostRequest(newGoal);

        // Act
        var result = await _sut.CreateSavingsGoal(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var returnedGoal = createdResult.Value.Should().BeOfType<SavingsGoal>().Subject;
        returnedGoal.Name.Should().Be("New Car Fund");
        returnedGoal.TargetAmount.Should().Be(30000);
        createdResult.Location.Should().Contain("/api/savings-goals/");
    }

    [Fact]
    public async Task CreateSavingsGoal_WithNullBody_ShouldReturnBadRequest()
    {
        // Arrange
        var request = CreatePostRequest<SavingsGoal?>(null);

        // Act
        var result = await _sut.CreateSavingsGoal(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateSavingsGoal_OptionsRequest_ShouldReturnOkWithCorsHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "OPTIONS";
        var request = context.Request;

        // Act
        var result = await _sut.CreateSavingsGoal(request);

        // Assert
        result.Should().BeOfType<OkResult>();
        context.Response.Headers["Access-Control-Allow-Origin"].ToString().Should().Be("*");
        context.Response.Headers["Access-Control-Allow-Methods"].ToString().Should().Contain("POST");
        context.Response.Headers["Access-Control-Allow-Headers"].ToString().Should().Be("*");
    }

    [Fact]
    public async Task CreateSavingsGoal_ShouldAddGoalToDataService()
    {
        // Arrange
        var initialCount = _dataService.GetSavingsGoals().Count;
        var newGoal = new SavingsGoal
        {
            Name = "Persistence Check Goal",
            TargetAmount = 5000,
            CurrentAmount = 0,
            TargetDate = DateTime.UtcNow.AddYears(1),
            MonthlyContribution = 400,
            Color = "#10B981"
        };
        var request = CreatePostRequest(newGoal);

        // Act
        await _sut.CreateSavingsGoal(request);

        // Assert
        _dataService.GetSavingsGoals().Count.Should().BeGreaterThan(initialCount);
    }

    [Fact]
    public async Task CreateSavingsGoal_ShouldReturnGoalWithComputedProperties()
    {
        // Arrange
        var newGoal = new SavingsGoal
        {
            Name = "Computed Props Test",
            TargetAmount = 10000,
            CurrentAmount = 2500,
            TargetDate = DateTime.UtcNow.AddMonths(12),
            MonthlyContribution = 625,
            Color = "#F59E0B"
        };
        var request = CreatePostRequest(newGoal);

        // Act
        var result = await _sut.CreateSavingsGoal(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var returnedGoal = createdResult.Value.Should().BeOfType<SavingsGoal>().Subject;
        returnedGoal.ProgressPercentage.Should().Be(25);
        returnedGoal.MonthsRemaining.Should().BeGreaterThan(0);
    }

    // ── Helpers ─────────────────────────────────────────────────

    private static HttpRequest CreateGetRequest()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        return context.Request;
    }

    private static HttpRequest CreatePostRequest<T>(T body)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.ContentType = "application/json";
        var json = JsonSerializer.Serialize(body);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return context.Request;
    }
}
