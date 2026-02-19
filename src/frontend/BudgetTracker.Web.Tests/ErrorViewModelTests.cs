using BudgetTracker.Web.Models;
using FluentAssertions;
using Xunit;

namespace BudgetTracker.Web.Tests;

public class ErrorViewModelTests
{
    // ── ShowRequestId ───────────────────────────────────────────

    [Fact]
    public void ShowRequestId_ShouldBeTrue_WhenRequestIdIsSet()
    {
        // Arrange
        var model = new ErrorViewModel { RequestId = "abc-123" };

        // Act
        var result = model.ShowRequestId;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShowRequestId_ShouldBeFalse_WhenRequestIdIsNull()
    {
        // Arrange
        var model = new ErrorViewModel { RequestId = null };

        // Act
        var result = model.ShowRequestId;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShowRequestId_ShouldBeFalse_WhenRequestIdIsEmpty()
    {
        // Arrange
        var model = new ErrorViewModel { RequestId = "" };

        // Act
        var result = model.ShowRequestId;

        // Assert
        result.Should().BeFalse();
    }
}
