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

public class DashboardControllerTests
{
    private readonly DashboardController _sut;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly BudgetApiClient _apiClient;

    public DashboardControllerTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:7071/api/")
        };
        _apiClient = new BudgetApiClient(httpClient);
        _sut = new DashboardController(_apiClient);
    }

    // ── Index ───────────────────────────────────────────────────

    [Fact]
    public async Task Index_ShouldReturnViewResult()
    {
        // Arrange
        SetupGetResponse("budgets", new List<Budget>
        {
            new() { Name = "December Budget", TotalAmount = 4500 }
        });
        SetupGetResponse("transactions", new List<Transaction>
        {
            new() { Description = "Groceries", Amount = 50m, Type = TransactionType.Expense }
        });
        SetupGetResponse("savings-goals", new List<SavingsGoal>
        {
            new() { Name = "Emergency Fund", TargetAmount = 10000, CurrentAmount = 5000 }
        });

        // Act
        var result = await _sut.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Index_ShouldPopulateViewBag_WithBudgetTransactionsAndGoals()
    {
        // Arrange
        var budgets = new List<Budget>
        {
            new() { Name = "Test Budget", TotalAmount = 3000 }
        };
        var transactions = new List<Transaction>
        {
            new() { Description = "Purchase 1", Amount = 25m, Type = TransactionType.Expense },
            new() { Description = "Purchase 2", Amount = 75m, Type = TransactionType.Expense }
        };
        var goals = new List<SavingsGoal>
        {
            new() { Name = "Goal 1", TargetAmount = 5000, CurrentAmount = 1000 }
        };

        SetupGetResponse("budgets", budgets);
        SetupGetResponse("transactions", transactions);
        SetupGetResponse("savings-goals", goals);

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;

        var budget = _sut.ViewBag.Budget as Budget;
        budget.Should().NotBeNull();
        budget!.Name.Should().Be("Test Budget");

        var viewTransactions = _sut.ViewBag.Transactions as List<Transaction>;
        viewTransactions.Should().NotBeNull();
        viewTransactions.Should().HaveCount(2);

        var viewGoals = _sut.ViewBag.SavingsGoals as List<SavingsGoal>;
        viewGoals.Should().NotBeNull();
        viewGoals.Should().HaveCount(1);
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
}
