using BudgetTracker.Web.Models;
using FluentAssertions;
using Xunit;

namespace BudgetTracker.Web.Tests;

public class SavingsGoalModelTests
{
    // ── ProgressPercentage ──────────────────────────────────────

    [Fact]
    public void ProgressPercentage_ShouldBeZero_WhenNoProgress()
    {
        // Arrange
        var goal = new SavingsGoal
        {
            Name = "Emergency Fund",
            TargetAmount = 10000,
            CurrentAmount = 0
        };

        // Act
        var progress = goal.ProgressPercentage;

        // Assert
        progress.Should().Be(0);
    }

    [Fact]
    public void ProgressPercentage_ShouldBe50_WhenHalfway()
    {
        // Arrange
        var goal = new SavingsGoal
        {
            Name = "Vacation",
            TargetAmount = 5000,
            CurrentAmount = 2500
        };

        // Act
        var progress = goal.ProgressPercentage;

        // Assert
        progress.Should().Be(50);
    }

    [Fact]
    public void ProgressPercentage_ShouldBe100_WhenGoalReached()
    {
        // Arrange
        var goal = new SavingsGoal
        {
            Name = "Laptop",
            TargetAmount = 2000,
            CurrentAmount = 2000
        };

        // Act
        var progress = goal.ProgressPercentage;

        // Assert
        progress.Should().Be(100);
    }

    [Fact]
    public void ProgressPercentage_CanExceed100_WhenOverTarget()
    {
        // Arrange
        var goal = new SavingsGoal
        {
            Name = "Bonus Savings",
            TargetAmount = 1000,
            CurrentAmount = 1500
        };

        // Act
        var progress = goal.ProgressPercentage;

        // Assert
        progress.Should().Be(150);
    }

    [Fact]
    public void ProgressPercentage_ShouldBeZero_WhenTargetAmountIsZero()
    {
        // Arrange
        var goal = new SavingsGoal
        {
            Name = "Empty Target",
            TargetAmount = 0,
            CurrentAmount = 500
        };

        // Act
        var progress = goal.ProgressPercentage;

        // Assert
        progress.Should().Be(0);
    }

    // ── MonthsRemaining ─────────────────────────────────────────

    [Fact]
    public void MonthsRemaining_ShouldBePositive_WhenTargetDateInFuture()
    {
        // Arrange
        var goal = new SavingsGoal
        {
            Name = "Future Goal",
            TargetDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var months = goal.MonthsRemaining;

        // Assert
        months.Should().BeGreaterThan(0);
        months.Should().BeLessThanOrEqualTo(7);
    }

    [Fact]
    public void MonthsRemaining_ShouldBeZero_WhenTargetDatePassed()
    {
        // Arrange
        var goal = new SavingsGoal
        {
            Name = "Past Goal",
            TargetDate = DateTime.UtcNow.AddDays(-30)
        };

        // Act
        var months = goal.MonthsRemaining;

        // Assert
        months.Should().Be(0);
    }
}
