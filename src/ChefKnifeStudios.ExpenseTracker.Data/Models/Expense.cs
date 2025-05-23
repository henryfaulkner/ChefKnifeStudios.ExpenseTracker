using ChefKnifeStudios.ExpenseTracker.Data.Repos;

namespace ChefKnifeStudios.ExpenseTracker.Data.Models;

public class Expense : BaseEntity
{
    public int BudgetId { get; set; }
    public required string Name { get; set; }
    public decimal Cost { get; set; }

    public Budget? Budget { get; set; }
}
