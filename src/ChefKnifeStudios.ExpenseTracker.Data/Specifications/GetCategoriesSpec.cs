using Ardalis.Specification;
using ChefKnifeStudios.ExpenseTracker.Data.Models;

namespace ChefKnifeStudios.ExpenseTracker.Data.Specifications;

public sealed class GetCategoriesSpec : Specification<Category>
{
    public GetCategoriesSpec(Guid appId)
    {
        Query
            .Where(x => x.AppId == appId
                || x.AppId == Guid.Empty) // wildcard
            .OrderBy(x => x.DisplayName);
    }
}
