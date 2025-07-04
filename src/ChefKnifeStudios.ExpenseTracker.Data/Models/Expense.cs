using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace ChefKnifeStudios.ExpenseTracker.Data.Models;

public class Expense : BaseEntity
{
    public int BudgetId { get; set; }
    public required string Name { get; set; }
    public decimal Cost { get; set; }
    public required string LabelsJson { get; set; }
    public bool IsRecurring { get; set; }

    [ForeignKey(nameof(BudgetId))]
    public Budget? Budget { get; set; }
    public ExpenseSemantic? ExpenseSemantic { get; set; }
}
