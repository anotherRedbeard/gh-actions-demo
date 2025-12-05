using BudgetTracker.Functions.Models;
using FluentAssertions;
using Xunit;

namespace BudgetTracker.Functions.Tests;

public class BudgetModelTests
{
    [Fact]
    public void Budget_ShouldHaveUniqueId_WhenCreated()
    {
        // Arrange & Act
        var budget1 = new Budget { Name = "Budget 1" };
        var budget2 = new Budget { Name = "Budget 2" };

        // Assert
        budget1.Id.Should().NotBeNullOrEmpty();
        budget2.Id.Should().NotBeNullOrEmpty();
        budget1.Id.Should().NotBe(budget2.Id);
    }

    [Fact]
    public void BudgetCategory_TotalSpent_ShouldMatchSpentAmount()
    {
        // Arrange
        var category = new BudgetCategory
        {
            Name = "Groceries",
            PlannedAmount = 500,
            SpentAmount = 375,
            Color = "#10B981"
        };

        // Act
        var percentage = (category.SpentAmount / category.PlannedAmount) * 100;

        // Assert
        percentage.Should().Be(75);
        category.SpentAmount.Should().BeLessThan(category.PlannedAmount);
    }

    [Fact]
    public void Budget_WithMultipleCategories_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var budget = new Budget
        {
            Name = "Test Budget",
            TotalAmount = 2000,
            Categories = new List<BudgetCategory>
            {
                new() { Name = "Cat1", PlannedAmount = 500, SpentAmount = 400, Color = "#fff" },
                new() { Name = "Cat2", PlannedAmount = 800, SpentAmount = 750, Color = "#fff" },
                new() { Name = "Cat3", PlannedAmount = 700, SpentAmount = 600, Color = "#fff" }
            }
        };

        // Act
        var totalPlanned = budget.Categories.Sum(c => c.PlannedAmount);
        var totalSpent = budget.Categories.Sum(c => c.SpentAmount);

        // Assert
        totalPlanned.Should().Be(2000);
        totalSpent.Should().Be(1750);
        (totalSpent / totalPlanned * 100).Should().Be(87.5m);
    }
}
