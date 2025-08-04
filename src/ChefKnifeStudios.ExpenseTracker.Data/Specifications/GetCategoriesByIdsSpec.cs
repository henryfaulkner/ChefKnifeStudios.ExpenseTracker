using Ardalis.Specification;
using ChefKnifeStudios.ExpenseTracker.Data.Models;

namespace ChefKnifeStudios.ExpenseTracker.Data.Specifications;

public sealed class GetCategoriesByIdsSpec : Specification<Category>
{
    public GetCategoriesByIdsSpec(List<int> categoryIds)
    {
        Query
            .Where(x => categoryIds.Contains(x.Id));
    }
}
