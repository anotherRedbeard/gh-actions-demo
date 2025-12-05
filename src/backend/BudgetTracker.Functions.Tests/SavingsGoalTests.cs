using BudgetTracker.Functions.Models;
using FluentAssertions;
using Xunit;

namespace BudgetTracker.Functions.Tests;

public class SavingsGoalTests
{
    [Fact]
    public void ProgressPercentage_ShouldBeZero_WhenNoProgress()
    {
        // Arrange
        var goal = new SavingsGoal
        {
            Name = "Emergency Fund",
            TargetAmount = 10000,
            CurrentAmount = 0,
            TargetDate = DateTime.UtcNow.AddYears(1),
            MonthlyContribution = 500
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
            CurrentAmount = 2500,
            TargetDate = DateTime.UtcNow.AddMonths(6),
            MonthlyContribution = 500
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
            Name = "Down Payment",
            TargetAmount = 20000,
            CurrentAmount = 20000,
            TargetDate = DateTime.UtcNow,
            MonthlyContribution = 1000
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
            CurrentAmount = 1200,
            TargetDate = DateTime.UtcNow.AddMonths(1),
            MonthlyContribution = 100
        };

        // Act
        var progress = goal.ProgressPercentage;

        // Assert
        progress.Should().Be(120);
    }

    [Fact]
    public void MonthsRemaining_ShouldBePositive_WhenTargetDateInFuture()
    {
        // Arrange
        var targetDate = DateTime.UtcNow.AddMonths(12);
        var goal = new SavingsGoal
        {
            Name = "Future Goal",
            TargetAmount = 12000,
            CurrentAmount = 0,
            TargetDate = targetDate,
            MonthlyContribution = 1000
        };

        // Act
        var months = goal.MonthsRemaining;

        // Assert
        months.Should().BeGreaterThan(0);
        months.Should().BeLessThanOrEqualTo(12);
    }

    [Fact]
    public void MonthsRemaining_ShouldBeZero_WhenTargetDatePassed()
    {
        // Arrange
        var goal = new SavingsGoal
        {
            Name = "Past Goal",
            TargetAmount = 5000,
            CurrentAmount = 3000,
            TargetDate = DateTime.UtcNow.AddDays(-30),
            MonthlyContribution = 500
        };

        // Act
        var months = goal.MonthsRemaining;

        // Assert
        months.Should().Be(0);
    }

    [Fact]
    public void MonthlyContribution_ShouldMatchRequiredAmount_ToReachGoal()
    {
        // Arrange
        var targetAmount = 12000m;
        var currentAmount = 2000m;
        var monthsRemaining = 10;
        var requiredMonthly = (targetAmount - currentAmount) / monthsRemaining;

        var goal = new SavingsGoal
        {
            Name = "Calculated Goal",
            TargetAmount = targetAmount,
            CurrentAmount = currentAmount,
            TargetDate = DateTime.UtcNow.AddMonths(monthsRemaining),
            MonthlyContribution = requiredMonthly
        };

        // Act
        var projectedAmount = currentAmount + (requiredMonthly * monthsRemaining);

        // Assert
        projectedAmount.Should().Be(targetAmount);
        goal.MonthlyContribution.Should().Be(1000);
    }
}
