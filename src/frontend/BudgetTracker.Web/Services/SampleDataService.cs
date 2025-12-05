using BudgetTracker.Web.Models;

namespace BudgetTracker.Web.Services;

public class SampleDataService
{
    private static readonly List<Budget> _budgets = new();
    private static readonly List<Transaction> _transactions = new();
    private static readonly List<SavingsGoal> _savingsGoals = new();

    static SampleDataService()
    {
        InitializeSampleData();
    }

    private static void InitializeSampleData()
    {
        // Sample Budget
        _budgets.Add(new Budget
        {
            Name = "December 2025 Budget",
            Month = new DateTime(2025, 12, 1),
            TotalAmount = 4500,
            Categories = new List<BudgetCategory>
            {
                new() { Name = "Groceries", PlannedAmount = 600, SpentAmount = 425, Color = "#10B981" },
                new() { Name = "Utilities", PlannedAmount = 300, SpentAmount = 285, Color = "#3B82F6" },
                new() { Name = "Entertainment", PlannedAmount = 200, SpentAmount = 150, Color = "#8B5CF6" },
                new() { Name = "Transportation", PlannedAmount = 400, SpentAmount = 320, Color = "#F59E0B" },
                new() { Name = "Dining Out", PlannedAmount = 300, SpentAmount = 280, Color = "#EF4444" },
                new() { Name = "Shopping", PlannedAmount = 500, SpentAmount = 450, Color = "#EC4899" },
                new() { Name = "Healthcare", PlannedAmount = 200, SpentAmount = 0, Color = "#06B6D4" },
                new() { Name = "Savings", PlannedAmount = 2000, SpentAmount = 2000, Color = "#14B8A6" }
            }
        });

        // Sample Transactions
        _transactions.AddRange(new[]
        {
            new Transaction { Description = "Grocery Shopping", Amount = 125.50m, Category = "Groceries", Date = DateTime.UtcNow.AddDays(-2), Type = TransactionType.Expense },
            new Transaction { Description = "Monthly Salary", Amount = 5000m, Category = "Income", Date = DateTime.UtcNow.AddDays(-28), Type = TransactionType.Income },
            new Transaction { Description = "Electric Bill", Amount = 145m, Category = "Utilities", Date = DateTime.UtcNow.AddDays(-5), Type = TransactionType.Expense },
            new Transaction { Description = "Gas Station", Amount = 55m, Category = "Transportation", Date = DateTime.UtcNow.AddDays(-3), Type = TransactionType.Expense },
            new Transaction { Description = "Movie Tickets", Amount = 45m, Category = "Entertainment", Date = DateTime.UtcNow.AddDays(-7), Type = TransactionType.Expense },
            new Transaction { Description = "Restaurant", Amount = 85m, Category = "Dining Out", Date = DateTime.UtcNow.AddDays(-1), Type = TransactionType.Expense },
            new Transaction { Description = "Water Bill", Amount = 65m, Category = "Utilities", Date = DateTime.UtcNow.AddDays(-10), Type = TransactionType.Expense },
            new Transaction { Description = "Grocery Shopping", Amount = 180m, Category = "Groceries", Date = DateTime.UtcNow.AddDays(-9), Type = TransactionType.Expense },
            new Transaction { Description = "Online Shopping", Amount = 225m, Category = "Shopping", Date = DateTime.UtcNow.AddDays(-4), Type = TransactionType.Expense },
            new Transaction { Description = "Gas Station", Amount = 60m, Category = "Transportation", Date = DateTime.UtcNow.AddDays(-8), Type = TransactionType.Expense },
        });

        // Sample Savings Goals
        _savingsGoals.AddRange(new[]
        {
            new SavingsGoal 
            { 
                Name = "Emergency Fund", 
                TargetAmount = 10000, 
                CurrentAmount = 6500, 
                TargetDate = new DateTime(2026, 12, 31),
                MonthlyContribution = 500,
                Color = "#10B981"
            },
            new SavingsGoal 
            { 
                Name = "Vacation to Europe", 
                TargetAmount = 5000, 
                CurrentAmount = 2800, 
                TargetDate = new DateTime(2026, 6, 1),
                MonthlyContribution = 400,
                Color = "#3B82F6"
            },
            new SavingsGoal 
            { 
                Name = "New Laptop", 
                TargetAmount = 2000, 
                CurrentAmount = 1200, 
                TargetDate = new DateTime(2026, 3, 1),
                MonthlyContribution = 300,
                Color = "#8B5CF6"
            }
        });
    }

    public List<Budget> GetBudgets() => _budgets;
    public Budget? GetBudget(string id) => _budgets.FirstOrDefault(b => b.Id == id);
    public void AddBudget(Budget budget) => _budgets.Add(budget);
    
    public List<Transaction> GetTransactions() => _transactions.OrderByDescending(t => t.Date).ToList();
    public Transaction? GetTransaction(string id) => _transactions.FirstOrDefault(t => t.Id == id);
    public void AddTransaction(Transaction transaction) => _transactions.Add(transaction);
    
    public List<SavingsGoal> GetSavingsGoals() => _savingsGoals;
    public SavingsGoal? GetSavingsGoal(string id) => _savingsGoals.FirstOrDefault(g => g.Id == id);
    public void AddSavingsGoal(SavingsGoal goal) => _savingsGoals.Add(goal);
}
