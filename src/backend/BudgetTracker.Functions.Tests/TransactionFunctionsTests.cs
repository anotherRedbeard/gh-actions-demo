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

public class TransactionFunctionsTests
{
    private readonly TransactionFunctions _sut;
    private readonly DataService _dataService;
    private readonly Mock<ILogger<TransactionFunctions>> _loggerMock;

    public TransactionFunctionsTests()
    {
        _loggerMock = new Mock<ILogger<TransactionFunctions>>();
        _dataService = new DataService();
        _sut = new TransactionFunctions(_loggerMock.Object, _dataService);
    }

    // ── GetTransactions ─────────────────────────────────────────

    [Fact]
    public void GetTransactions_ShouldReturnOkResult_WithListOfTransactions()
    {
        // Arrange
        var request = CreateGetRequest();

        // Act
        var result = _sut.GetTransactions(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var transactions = okResult.Value.Should().BeAssignableTo<List<Transaction>>().Subject;
        transactions.Should().NotBeEmpty();
    }

    [Fact]
    public void GetTransactions_ShouldReturnTransactionsOrderedByDateDescending()
    {
        // Arrange
        var request = CreateGetRequest();

        // Act
        var result = _sut.GetTransactions(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var transactions = okResult.Value.Should().BeAssignableTo<List<Transaction>>().Subject;
        transactions.Should().BeInDescendingOrder(t => t.Date);
    }

    [Fact]
    public void GetTransactions_ShouldSetCorsHeaders()
    {
        // Arrange
        var request = CreateGetRequest();

        // Act
        _sut.GetTransactions(request);

        // Assert
        request.HttpContext.Response.Headers["Access-Control-Allow-Origin"].ToString().Should().Be("*");
    }

    // ── GetTransaction ──────────────────────────────────────────

    [Fact]
    public void GetTransaction_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var existingTransaction = _dataService.GetTransactions().First();
        var request = CreateGetRequest();

        // Act
        var result = _sut.GetTransaction(request, existingTransaction.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var transaction = okResult.Value.Should().BeOfType<Transaction>().Subject;
        transaction.Id.Should().Be(existingTransaction.Id);
    }

    [Fact]
    public void GetTransaction_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var request = CreateGetRequest();

        // Act
        var result = _sut.GetTransaction(request, "nonexistent-id");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void GetTransaction_ShouldSetCorsHeaders()
    {
        // Arrange
        var existingTransaction = _dataService.GetTransactions().First();
        var request = CreateGetRequest();

        // Act
        _sut.GetTransaction(request, existingTransaction.Id);

        // Assert
        request.HttpContext.Response.Headers["Access-Control-Allow-Origin"].ToString().Should().Be("*");
    }

    // ── CreateTransaction ───────────────────────────────────────

    [Fact]
    public async Task CreateTransaction_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        var newTransaction = new Transaction
        {
            Description = "Test Purchase",
            Amount = 42.50m,
            Category = "Shopping",
            Date = DateTime.UtcNow,
            Type = TransactionType.Expense
        };
        var request = CreatePostRequest(newTransaction);

        // Act
        var result = await _sut.CreateTransaction(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var returnedTransaction = createdResult.Value.Should().BeOfType<Transaction>().Subject;
        returnedTransaction.Description.Should().Be("Test Purchase");
        returnedTransaction.Amount.Should().Be(42.50m);
        createdResult.Location.Should().Contain("/api/transactions/");
    }

    [Fact]
    public async Task CreateTransaction_WithNullBody_ShouldReturnBadRequest()
    {
        // Arrange
        var request = CreatePostRequest<Transaction?>(null);

        // Act
        var result = await _sut.CreateTransaction(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateTransaction_OptionsRequest_ShouldReturnOkWithCorsHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "OPTIONS";
        var request = context.Request;

        // Act
        var result = await _sut.CreateTransaction(request);

        // Assert
        result.Should().BeOfType<OkResult>();
        context.Response.Headers["Access-Control-Allow-Origin"].ToString().Should().Be("*");
        context.Response.Headers["Access-Control-Allow-Methods"].ToString().Should().Contain("POST");
        context.Response.Headers["Access-Control-Allow-Headers"].ToString().Should().Be("*");
    }

    [Fact]
    public async Task CreateTransaction_ShouldAddTransactionToDataService()
    {
        // Arrange
        var initialCount = _dataService.GetTransactions().Count;
        var newTransaction = new Transaction
        {
            Description = "Persistence Check",
            Amount = 10m,
            Category = "Utilities",
            Date = DateTime.UtcNow,
            Type = TransactionType.Expense
        };
        var request = CreatePostRequest(newTransaction);

        // Act
        await _sut.CreateTransaction(request);

        // Assert
        _dataService.GetTransactions().Count.Should().BeGreaterThan(initialCount);
    }

    [Fact]
    public async Task CreateTransaction_WithIncomeType_ShouldReturnCreatedResult()
    {
        // Arrange
        var incomeTransaction = new Transaction
        {
            Description = "Freelance Payment",
            Amount = 1500m,
            Category = "Income",
            Date = DateTime.UtcNow,
            Type = TransactionType.Income
        };
        var request = CreatePostRequest(incomeTransaction);

        // Act
        var result = await _sut.CreateTransaction(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var returnedTransaction = createdResult.Value.Should().BeOfType<Transaction>().Subject;
        returnedTransaction.Type.Should().Be(TransactionType.Income);
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
