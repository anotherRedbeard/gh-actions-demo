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

public class BudgetFunctionsTests
{
    private readonly BudgetFunctions _sut;
    private readonly DataService _dataService;
    private readonly Mock<ILogger<BudgetFunctions>> _loggerMock;

    public BudgetFunctionsTests()
    {
        _loggerMock = new Mock<ILogger<BudgetFunctions>>();
        _dataService = new DataService();
        _sut = new BudgetFunctions(_loggerMock.Object, _dataService);
    }

    // ── GetBudgets ──────────────────────────────────────────────

    [Fact]
    public void GetBudgets_ShouldReturnOkResult_WithListOfBudgets()
    {
        // Arrange
        var request = CreateGetRequest();

        // Act
        var result = _sut.GetBudgets(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var budgets = okResult.Value.Should().BeAssignableTo<List<Budget>>().Subject;
        budgets.Should().NotBeEmpty();
    }

    [Fact]
    public void GetBudgets_ShouldSetCorsHeaders()
    {
        // Arrange
        var request = CreateGetRequest();

        // Act
        _sut.GetBudgets(request);

        // Assert
        request.HttpContext.Response.Headers["Access-Control-Allow-Origin"].ToString().Should().Be("*");
        request.HttpContext.Response.Headers["Access-Control-Allow-Methods"].ToString().Should().Contain("GET");
    }

    // ── GetBudget ───────────────────────────────────────────────

    [Fact]
    public void GetBudget_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var existingBudget = _dataService.GetBudgets().First();
        var request = CreateGetRequest();

        // Act
        var result = _sut.GetBudget(request, existingBudget.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var budget = okResult.Value.Should().BeOfType<Budget>().Subject;
        budget.Id.Should().Be(existingBudget.Id);
    }

    [Fact]
    public void GetBudget_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var request = CreateGetRequest();

        // Act
        var result = _sut.GetBudget(request, "nonexistent-id");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    // ── CreateBudget ────────────────────────────────────────────

    [Fact]
    public async Task CreateBudget_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        var newBudget = new Budget
        {
            Name = "Test Budget",
            Month = new DateTime(2026, 3, 1),
            TotalAmount = 3000,
            Categories = new List<BudgetCategory>
            {
                new() { Name = "Food", PlannedAmount = 500, SpentAmount = 0, Color = "#10B981" }
            }
        };
        var request = CreatePostRequest(newBudget);

        // Act
        var result = await _sut.CreateBudget(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var returnedBudget = createdResult.Value.Should().BeOfType<Budget>().Subject;
        returnedBudget.Name.Should().Be("Test Budget");
        createdResult.Location.Should().Contain("/api/budgets/");
    }

    [Fact]
    public async Task CreateBudget_WithNullBody_ShouldReturnBadRequest()
    {
        // Arrange
        var request = CreatePostRequest<Budget?>(null);

        // Act
        var result = await _sut.CreateBudget(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateBudget_OptionsRequest_ShouldReturnOkWithCorsHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "OPTIONS";
        var request = context.Request;

        // Act
        var result = await _sut.CreateBudget(request);

        // Assert
        result.Should().BeOfType<OkResult>();
        context.Response.Headers["Access-Control-Allow-Origin"].ToString().Should().Be("*");
        context.Response.Headers["Access-Control-Allow-Methods"].ToString().Should().Contain("POST");
        context.Response.Headers["Access-Control-Allow-Headers"].ToString().Should().Be("*");
    }

    [Fact]
    public async Task CreateBudget_ShouldAddBudgetToDataService()
    {
        // Arrange
        var initialCount = _dataService.GetBudgets().Count;
        var newBudget = new Budget
        {
            Name = "Persistence Check Budget",
            Month = new DateTime(2026, 5, 1),
            TotalAmount = 1500,
            Categories = new List<BudgetCategory>()
        };
        var request = CreatePostRequest(newBudget);

        // Act
        await _sut.CreateBudget(request);

        // Assert
        _dataService.GetBudgets().Count.Should().BeGreaterThan(initialCount);
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
