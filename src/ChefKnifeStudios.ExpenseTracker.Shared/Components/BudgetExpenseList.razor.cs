using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components;

public partial class BudgetExpenseList : ComponentBase
{
    [Inject] ISearchViewModel SearchViewModel { get; set; } = null!;

    PaginationState _pagination = new PaginationState { ItemsPerPage = 10 };

    static string FormatAsDollar(decimal amount)
    {
        return amount.ToString("C", CultureInfo.GetCultureInfo("en-US"));
    }
}
