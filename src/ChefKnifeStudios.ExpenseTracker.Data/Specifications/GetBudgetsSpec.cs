using Ardalis.Specification;
using ChefKnifeStudios.ExpenseTracker.Data.Models;

namespace ChefKnifeStudios.ExpenseTracker.Data.Specifications;

public sealed class GetBudgetsSpec : Specification<Budget>
{
    public GetBudgetsSpec(Guid appId)
    {
        Query
            .Include(x => x.Expenses)
                .ThenInclude(x => x.ExpenseCategories)
                    .ThenInclude(x => x.Category)
            .Where(x => x.AppId == appId)
            .OrderByDescending(x => x.CreatedOnUtc);
    }
}
