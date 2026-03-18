using ChefKnifeStudios.ExpenseTracker.Shared.Services;

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
