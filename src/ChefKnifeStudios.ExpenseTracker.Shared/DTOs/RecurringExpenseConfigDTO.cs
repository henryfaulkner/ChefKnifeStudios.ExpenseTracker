namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class RecurringExpenseConfigDTO 
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Cost { get; set; }
    public required IEnumerable<string> Labels { get; set; }
}
