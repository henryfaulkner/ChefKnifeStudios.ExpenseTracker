using ChefKnifeStudios.ExpenseTracker.Data.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.Data.Repos;

public interface IBudgetWithExpensesRepository : IReadRepository<BudgetWithExpenses>
{
    BudgetWithExpenses GetResultAsync();
}

public class BudgetWithExpensesRepository : EfReadRepository<BudgetWithExpenses>, IBudgetWithExpensesRepository
{
    readonly AppDbContext _dbContext;

    public BudgetWithExpensesRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BudgetWithExpenses> GetResultAsync()
    {
        var viewResult = _dbContext.Set<IEnumerable<BudgetWithExpenses.ViewResult>>();


    }
}
