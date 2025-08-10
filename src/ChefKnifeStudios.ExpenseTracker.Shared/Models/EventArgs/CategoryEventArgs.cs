using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;

public class CategoryEventArgs : IEventArgs
{
    public enum EventTypes { AddExpenseCategories, }

    public class EventData
    {
        public required IEnumerable<CategoryDTO> Categories { get; init; }
    }

    public required EventTypes Type { get; init; }
    public EventData? Data { get; init; }
}
