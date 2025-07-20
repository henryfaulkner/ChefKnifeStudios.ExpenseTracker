namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class ExpenseDTO 
{
    public int Id { get; set; } = 0;
    public int BudgetId { get; set; }
    public decimal Cost { get; set; }
    public required string Name { get; set; }
    public required IEnumerable<string> Labels { get; set; }
    public bool IsRecurring { get; set; }
    public BudgetDTO? Budget { get; set; }
    public ExpenseSemanticDTO? ExpenseSemantic { get; set; }
}
