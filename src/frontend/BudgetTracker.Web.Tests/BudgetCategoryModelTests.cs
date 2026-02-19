using BudgetTracker.Web.Models;
using FluentAssertions;
using Xunit;

namespace BudgetTracker.Web.Tests;

public class BudgetCategoryModelTests
{
    [Fact]
    public void BudgetCategory_DefaultValues_ShouldBeSet()
    {
        // Act
        var category = new BudgetCategory();

        // Assert
        category.Name.Should().Be(string.Empty);
        category.PlannedAmount.Should().Be(0);
        category.SpentAmount.Should().Be(0);
        category.Color.Should().Be("#1E3A8A");
    }

    [Fact]
    public void BudgetCategory_ShouldStoreAssignedValues()
    {
        // Arrange & Act
        var category = new BudgetCategory
        {
            Name = "Groceries",
            PlannedAmount = 600,
            SpentAmount = 425,
            Color = "#10B981"
        };

        // Assert
        category.Name.Should().Be("Groceries");
        category.PlannedAmount.Should().Be(600);
        category.SpentAmount.Should().Be(425);
        category.Color.Should().Be("#10B981");
    }

    [Fact]
    public void Budget_ShouldInitializeWithEmptyCategoriesList()
    {
        // Act
        var budget = new Budget();

        // Assert
        budget.Categories.Should().NotBeNull();
        budget.Categories.Should().BeEmpty();
        budget.Id.Should().NotBeNullOrWhiteSpace();
    }
}
