namespace ChefKnifeStudios.ExpenseTracker.Data.Models;

public class Expense : BaseEntity
{
    public int BudgetId { get; set; }
    public required string Name { get; set; }
    public decimal Cost { get; set; }
    public required string LabelsJson { get; set; }
    public bool IsRecurring { get; set; }
    public Guid AppId { get; set; }

    public Budget? Budget { get; set; }
    public ExpenseSemantic? ExpenseSemantic { get; set; }
    public ICollection<ExpenseCategory>? ExpenseCategories { get; set; }
}
