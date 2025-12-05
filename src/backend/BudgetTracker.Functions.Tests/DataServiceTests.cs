using BudgetTracker.Functions.Models;
using BudgetTracker.Functions.Services;
using FluentAssertions;
using Xunit;

namespace BudgetTracker.Functions.Tests;

public class DataServiceTests
{
    [Fact]
    public void AddTransaction_ShouldAddToList()
    {
        // Arrange
        var dataService = new DataService();
        var initialCount = dataService.GetTransactions().Count;
        var newTransaction = new Transaction
        {
            Description = "Test Purchase",
            Amount = 50.00m,
            Category = "Groceries",
            Date = DateTime.UtcNow,
            Type = TransactionType.Expense
        };

        // Act
        dataService.AddTransaction(newTransaction);

        // Assert
        var transactions = dataService.GetTransactions();
        transactions.Should().HaveCount(initialCount + 1);
        transactions.Should().ContainEquivalentOf(newTransaction);
    }

    [Fact]
    public void GetBudgets_ShouldReturnNonEmptyList()
    {
        // Arrange
        var dataService = new DataService();

        // Act
        var budgets = dataService.GetBudgets();

        // Assert
        budgets.Should().NotBeEmpty();
        budgets.Should().AllSatisfy(b =>
        {
            b.Id.Should().NotBeNullOrEmpty();
            b.Name.Should().NotBeNullOrEmpty();
            b.Categories.Should().NotBeNull();
        });
    }

    [Fact]
    public void GetBudget_WithValidId_ShouldReturnBudget()
    {
        // Arrange
        var dataService = new DataService();
        var allBudgets = dataService.GetBudgets();
        var existingBudget = allBudgets.First();

        // Act
        var budget = dataService.GetBudget(existingBudget.Id);

        // Assert
        budget.Should().NotBeNull();
        budget!.Id.Should().Be(existingBudget.Id);
        budget.Name.Should().Be(existingBudget.Name);
    }

    [Fact]
    public void GetBudget_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var dataService = new DataService();
        var invalidId = Guid.NewGuid().ToString();

        // Act
        var budget = dataService.GetBudget(invalidId);

        // Assert
        budget.Should().BeNull();
    }

    [Fact]
    public void AddBudget_ShouldAddToList()
    {
        // Arrange
        var dataService = new DataService();
        var initialCount = dataService.GetBudgets().Count;
        var newBudget = new Budget
        {
            Name = "Test Budget",
            Month = new DateTime(2026, 1, 1),
            TotalAmount = 3000,
            Categories = new List<BudgetCategory>
            {
                new() { Name = "Food", PlannedAmount = 500, SpentAmount = 0, Color = "#10B981" }
            }
        };

        // Act
        dataService.AddBudget(newBudget);

        // Assert
        var budgets = dataService.GetBudgets();
        budgets.Should().HaveCount(initialCount + 1);
        budgets.Should().ContainEquivalentOf(newBudget);
    }

    [Fact]
    public void GetTransactions_ShouldReturnOrderedByDateDescending()
    {
        // Arrange
        var dataService = new DataService();

        // Act
        var transactions = dataService.GetTransactions();

        // Assert
        transactions.Should().NotBeEmpty();
        transactions.Should().BeInDescendingOrder(t => t.Date);
    }

    [Fact]
    public void GetSavingsGoals_ShouldReturnNonEmptyList()
    {
        // Arrange
        var dataService = new DataService();

        // Act
        var goals = dataService.GetSavingsGoals();

        // Assert
        goals.Should().NotBeEmpty();
        goals.Should().AllSatisfy(g =>
        {
            g.Id.Should().NotBeNullOrEmpty();
            g.Name.Should().NotBeNullOrEmpty();
            g.TargetAmount.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public void AddSavingsGoal_ShouldAddToList()
    {
        // Arrange
        var dataService = new DataService();
        var initialCount = dataService.GetSavingsGoals().Count;
        var newGoal = new SavingsGoal
        {
            Name = "New Car",
            TargetAmount = 25000,
            CurrentAmount = 5000,
            TargetDate = DateTime.UtcNow.AddYears(2),
            MonthlyContribution = 800,
            Color = "#3B82F6"
        };

        // Act
        dataService.AddSavingsGoal(newGoal);

        // Assert
        var goals = dataService.GetSavingsGoals();
        goals.Should().HaveCount(initialCount + 1);
        goals.Should().ContainEquivalentOf(newGoal);
    }

    [Fact]
    public void AddTransaction_ShouldUpdateBudgetCategorySpentAmount_WhenCategoryMatches()
    {
        // Arrange
        var dataService = new DataService();
        var budget = dataService.GetBudgets().First(b => b.Name == "December 2025 Budget");
        var groceryCategory = budget.Categories.First(c => c.Name == "Groceries");
        var initialSpent = groceryCategory.SpentAmount;

        var transaction = new Transaction
        {
            Description = "Test Walmart Purchase",
            Amount = 75.50m,
            Category = "Groceries",
            Date = new DateTime(2025, 12, 15),
            Type = TransactionType.Expense
        };

        // Act
        Console.WriteLine($"Before: Budget ID={budget.Id}, Groceries Spent={initialSpent}");
        Console.WriteLine($"Transaction: Date={transaction.Date}, Category={transaction.Category}, Amount={transaction.Amount}");
        dataService.AddTransaction(transaction);

        // Assert
        var updatedBudget = dataService.GetBudget(budget.Id);
        var updatedCategory = updatedBudget!.Categories.First(c => c.Name == "Groceries");
        Console.WriteLine($"After: Groceries Spent={updatedCategory.SpentAmount}");
        Console.WriteLine($"Expected: {initialSpent + 75.50m}");
        updatedCategory.SpentAmount.Should().Be(initialSpent + 75.50m);
    }

    [Fact]
    public void AddTransaction_ShouldNotUpdateBudget_WhenNoMatchingCategoryExists()
    {
        // Arrange
        var dataService = new DataService();
        var budget = new Budget
        {
            Name = "January 2026 Budget",
            Month = new DateTime(2026, 1, 1),
            TotalAmount = 1000,
            Categories = new List<BudgetCategory>
            {
                new() { Name = "Groceries", PlannedAmount = 500, SpentAmount = 100, Color = "#10B981" }
            }
        };
        dataService.AddBudget(budget);

        var transaction = new Transaction
        {
            Description = "Netflix",
            Amount = 15.99m,
            Category = "Entertainment", // No matching category
            Date = new DateTime(2025, 12, 15),
            Type = TransactionType.Expense
        };

        // Act
        dataService.AddTransaction(transaction);

        // Assert
        var updatedBudget = dataService.GetBudget(budget.Id);
        var groceryCategory = updatedBudget!.Categories.First(c => c.Name == "Groceries");
        groceryCategory.SpentAmount.Should().Be(100); // Unchanged
    }

    [Fact]
    public void AddTransaction_ShouldOnlyUpdateBudgetForMatchingMonth()
    {
        // Arrange
        var dataService = new DataService();
        var februaryBudget = new Budget
        {
            Name = "February 2026 Budget",
            Month = new DateTime(2026, 2, 1),
            TotalAmount = 1000,
            Categories = new List<BudgetCategory>
            {
                new() { Name = "Groceries", PlannedAmount = 500, SpentAmount = 100, Color = "#10B981" }
            }
        };
        var marchBudget = new Budget
        {
            Name = "March 2026 Budget",
            Month = new DateTime(2026, 3, 1),
            TotalAmount = 1000,
            Categories = new List<BudgetCategory>
            {
                new() { Name = "Groceries", PlannedAmount = 500, SpentAmount = 50, Color = "#10B981" }
            }
        };
        dataService.AddBudget(februaryBudget);
        dataService.AddBudget(marchBudget);

        var transaction = new Transaction
        {
            Description = "Grocery Store",
            Amount = 80m,
            Category = "Groceries",
            Date = new DateTime(2026, 2, 20), // February transaction
            Type = TransactionType.Expense
        };

        // Act
        dataService.AddTransaction(transaction);

        // Assert
        var updatedFebBudget = dataService.GetBudget(februaryBudget.Id);
        var updatedMarBudget = dataService.GetBudget(marchBudget.Id);
        
        updatedFebBudget!.Categories.First().SpentAmount.Should().Be(180m); // 100 + 80
        updatedMarBudget!.Categories.First().SpentAmount.Should().Be(50m); // Unchanged
    }

    [Fact]
    public void AddTransaction_ShouldNotUpdateBudget_WhenTransactionIsIncome()
    {
        // Arrange
        var dataService = new DataService();
        var budget = new Budget
        {
            Name = "April 2026 Budget",
            Month = new DateTime(2026, 4, 1),
            TotalAmount = 1000,
            Categories = new List<BudgetCategory>
            {
                new() { Name = "Income", PlannedAmount = 0, SpentAmount = 0, Color = "#10B981" }
            }
        };
        dataService.AddBudget(budget);

        var transaction = new Transaction
        {
            Description = "Salary",
            Amount = 5000m,
            Category = "Income",
            Date = new DateTime(2026, 4, 1),
            Type = TransactionType.Income
        };

        // Act
        dataService.AddTransaction(transaction);

        // Assert
        var updatedBudget = dataService.GetBudget(budget.Id);
        var incomeCategory = updatedBudget!.Categories.First();
        incomeCategory.SpentAmount.Should().Be(0); // Unchanged - income shouldn't update budget
    }
}
