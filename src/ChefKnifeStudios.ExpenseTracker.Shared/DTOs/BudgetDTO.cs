namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class BudgetDTO
{
    public int Id { get; set; } = 0;
    public required string Name { get; set; }
    public decimal ExpenseBudget { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}