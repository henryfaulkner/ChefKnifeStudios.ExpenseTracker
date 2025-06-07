using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChefKnifeStudios.ExpenseTracker.Data.Models;

public class Expense : BaseEntity
{
    public int BudgetId { get; set; }
    public required string Name { get; set; }
    public decimal Cost { get; set; }
    public required string LabelsJson { get; set; }

    [ForeignKey(nameof(BudgetId))]
    public Budget? Budget { get; set; }
}
