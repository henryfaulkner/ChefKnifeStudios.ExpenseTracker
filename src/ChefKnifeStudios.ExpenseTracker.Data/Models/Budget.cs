using ChefKnifeStudios.ExpenseTracker.Data.Repos;

namespace ChefKnifeStudios.ExpenseTracker.Data.Models;

public class Budget : BaseEntity, IAggregateRoot
{
    public required string Name { get; set; }
    public decimal ExpenseBudget { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
}