using Ardalis.Specification;
using ChefKnifeStudios.ExpenseTracker.Data.Models;

namespace ChefKnifeStudios.ExpenseTracker.Data.Specifications;

public sealed class GetExpensesByIdsSpec : Specification<Expense>
{
    public GetExpensesByIdsSpec(List<int> expenseIds, Guid appId)
    {
        Query
            .Include(x => x.Budget)
            .Include(x => x.ExpenseCategories)
            .Where(x => x.AppId == appId)
            .Where(x => expenseIds.Contains(x.Id)); 
    }
}
