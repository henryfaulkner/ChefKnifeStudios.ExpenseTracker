using Ardalis.Specification;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ChefKnifeStudios.ExpenseTracker.Data.Specifications;

public sealed class GetBudgetsSpec : Specification<Budget>
{
    public GetBudgetsSpec(Guid appId)
    {
        Query
            .Include(x => x.Expenses)
            .Where(x => x.AppId == appId)
            .OrderByDescending(x => x.CreatedOnUtc);
    }
}
