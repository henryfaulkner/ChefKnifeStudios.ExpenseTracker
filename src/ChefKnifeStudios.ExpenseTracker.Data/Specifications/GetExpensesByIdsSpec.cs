﻿using Ardalis.Specification;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using System.Linq;

namespace ChefKnifeStudios.ExpenseTracker.Data.Specifications;

public sealed class GetExpensesByIdsSpec : Specification<Expense>
{
    public GetExpensesByIdsSpec(List<int> expenseIds, Guid appId)
    {
        Query
            .Include(x => x.Budget)
            .Where(x => x.AppId == appId)
            .Where(x => expenseIds.Contains(x.Id)); 
    }
}
