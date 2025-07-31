using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;

public class BladeEventArgs : IEventArgs
{
    public enum Types
    {
        Close,
        Expense,
        Budget,
        Search,
        ActiveRecurringExpenses,
        DownloadBudgets,
    }

    public required Types Type { get; init; }

    public object? Data { get; init; }
}
