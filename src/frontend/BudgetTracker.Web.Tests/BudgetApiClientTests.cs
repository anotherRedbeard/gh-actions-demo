using System.Net;
using System.Text.Json;
using BudgetTracker.Web.Models;
using BudgetTracker.Web.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace BudgetTracker.Web.Tests;

public class BudgetApiClientTests
{
    private readonly BudgetApiClient _sut;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;

    public BudgetApiClientTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:7071/api/")
        };
        _sut = new BudgetApiClient(httpClient);
    }

    // ── GetAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_ShouldDeserializeResponse_WhenSuccessful()
    {
        // Arrange
        var expectedBudgets = new List<Budget>
        {
            new() { Name = "Test Budget", TotalAmount = 3000 }
        };
        SetupGetResponse("budgets", expectedBudgets);

        // Act
        var result = await _sut.GetAsync<List<Budget>>("budgets");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result!.First().Name.Should().Be("Test Budget");
    }

    [Fact]
    public async Task GetAsync_ShouldThrow_WhenResponseIsNotSuccess()
    {
        // Arrange
        _httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act
        var act = () => _sut.GetAsync<List<Budget>>("budgets");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    // ── PostAsync ───────────────────────────────────────────────

    [Fact]
    public async Task PostAsync_ShouldDeserializeResponse_WhenSuccessful()
    {
        // Arrange
        var budget = new Budget { Name = "New Budget", TotalAmount = 5000 };
        SetupPostResponse("budgets", budget);

        // Act
        var result = await _sut.PostAsync<Budget>("budgets", budget);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Budget");
        result.TotalAmount.Should().Be(5000);
    }

    [Fact]
    public async Task PostAsync_ShouldSendJsonContent()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var budget = new Budget { Name = "Captured Budget", TotalAmount = 2000 };

        _httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(budget),
                    System.Text.Encoding.UTF8,
                    "application/json")
            });

        // Act
        await _sut.PostAsync<Budget>("budgets", budget);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.Content.Should().NotBeNull();

        var sentJson = await capturedRequest.Content!.ReadAsStringAsync();
        sentJson.Should().Contain("Captured Budget");
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
