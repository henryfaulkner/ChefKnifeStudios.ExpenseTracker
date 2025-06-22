namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class ExpenseSearchResponseDTO
{
    public int ExpenseId { get; set; }
    public required string ExpenseName { get; set; }
    public decimal Cost { get; set; }
    public required string BudgetName { get; set; }
}
