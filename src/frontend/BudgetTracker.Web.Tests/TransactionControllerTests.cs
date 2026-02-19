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

public class TransactionControllerTests
{
    private readonly TransactionController _sut;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly BudgetApiClient _apiClient;

    public TransactionControllerTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:7071/api/")
        };
        _apiClient = new BudgetApiClient(httpClient);
        _sut = new TransactionController(_apiClient);
    }

    // ── Index ───────────────────────────────────────────────────

    [Fact]
    public async Task Index_ShouldReturnViewResult_WithListOfTransactions()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new() { Description = "Groceries", Amount = 50m, Category = "Food", Type = TransactionType.Expense },
            new() { Description = "Salary", Amount = 3000m, Category = "Income", Type = TransactionType.Income }
        };
        SetupGetResponse("transactions", transactions);

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<List<Transaction>>().Subject;
        model.Should().HaveCount(2);
    }

    [Fact]
    public async Task Index_WhenApiReturnsNull_ShouldReturnEmptyList()
    {
        // Arrange
        SetupGetResponse<List<Transaction>?>("transactions", null);

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<List<Transaction>>().Subject;
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
        var transaction = new Transaction
        {
            Description = "New Purchase",
            Amount = 99.99m,
            Category = "Shopping",
            Type = TransactionType.Expense
        };
        SetupPostResponse("transactions", transaction);

        // Act
        var result = await _sut.Create(transaction);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Create_Post_WithInvalidModelState_ShouldReturnView()
    {
        // Arrange
        var transaction = new Transaction { Description = "" };
        _sut.ModelState.AddModelError("Description", "Description is required");

        // Act
        var result = await _sut.Create(transaction);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<Transaction>();
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
