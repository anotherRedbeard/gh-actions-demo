namespace BudgetTracker.Functions.Models;

public class Budget
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime Month { get; set; }
    public List<BudgetCategory> Categories { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class BudgetCategory
{
    public string Name { get; set; } = string.Empty;
    public decimal PlannedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public string Color { get; set; } = "#1E3A8A";
}
