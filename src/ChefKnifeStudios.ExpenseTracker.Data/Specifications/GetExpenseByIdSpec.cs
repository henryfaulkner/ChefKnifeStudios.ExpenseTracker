using Ardalis.Specification;
using ChefKnifeStudios.ExpenseTracker.Data.Models;

namespace ChefKnifeStudios.ExpenseTracker.Data.Specifications;

public sealed class GetExpenseByIdSpec : Specification<Expense>
{
    public GetExpenseByIdSpec(int expenseId, Guid appId)
    {
        Query
            .Include(x => x.Budget)
            .Include(x => x.ExpenseCategories)
            .Where(x => x.AppId == appId)
            .Where(x => x.Id == expenseId);
    }
}
