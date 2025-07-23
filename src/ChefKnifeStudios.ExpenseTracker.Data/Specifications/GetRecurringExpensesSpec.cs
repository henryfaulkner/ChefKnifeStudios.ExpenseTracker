using Ardalis.Specification;
using ChefKnifeStudios.ExpenseTracker.Data.Models;

namespace ChefKnifeStudios.ExpenseTracker.Data.Specifications;

public sealed class GetRecurringExpensesSpec : Specification<RecurringExpenseConfig>
{
    public GetRecurringExpensesSpec(Guid appId)
    {
        Query
            .Where(c => c.AppId == appId)
            .OrderByDescending(x => x.CreatedOnUtc);
    }
}
