using ChefKnifeStudios.ExpenseTracker.Shared.Services;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;

public class RecurringExpenseEventArgs : IEventArgs
{
    public enum Types { Added, }

    public required Types Type { get; init; }
}
