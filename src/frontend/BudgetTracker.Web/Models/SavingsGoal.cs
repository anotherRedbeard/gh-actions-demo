namespace BudgetTracker.Web.Models;

public class SavingsGoal
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public decimal MonthlyContribution { get; set; }
    public string Color { get; set; } = "#10B981";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public decimal ProgressPercentage => TargetAmount > 0 ? (CurrentAmount / TargetAmount) * 100 : 0;
    public int MonthsRemaining => TargetDate > DateTime.UtcNow ? (int)Math.Ceiling((TargetDate - DateTime.UtcNow).TotalDays / 30) : 0;
}
