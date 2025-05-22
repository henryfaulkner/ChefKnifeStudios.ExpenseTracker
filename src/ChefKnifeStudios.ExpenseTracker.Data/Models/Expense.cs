using ChefKnifeStudios.ExpenseTracker.Data.Repos;

namespace ChefKnifeStudios.ExpenseTracker.Data.Models;

public class Expense : BaseEntity, IAggregateRoot
{
    public decimal Cost { get; set; }
    public required string Name { get; set; }
}
