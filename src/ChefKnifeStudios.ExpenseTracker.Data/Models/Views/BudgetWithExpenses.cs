using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.Data.Models.Views;

public class BudgetWithExpenses : IReadAggregateRoot
{
    public class ViewResult
    {
        public int BudgetId { get; set; } 
        public decimal TotalAggregateExpenseCost { get; set; }
        public string? ExpenseIdsStr { get; set; }
    }

    public required Budget Budget { get; set; }
    public decimal TotalExpenseCost { get; set; }
    public required IEnumerable<Expense> Expenses { get; set; }   
}
