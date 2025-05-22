namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class BudgetWithExpensesDTO
{
    public required BudgetDTO Budget { get; set; }
    public decimal TotalExpenseCost { get; set; }
    public required IEnumerable<ExpenseDTO> Expenses { get; set; }
}