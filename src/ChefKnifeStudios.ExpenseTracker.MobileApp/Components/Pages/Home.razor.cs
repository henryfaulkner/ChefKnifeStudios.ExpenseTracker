using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] ISearchViewModel SearchViewModel { get; set; } = null!;
    [Inject] IRecurringExpenseViewModel RecurringExpenseViewModel { get; set; } = null!;
    [Inject] ICategoryViewModel CategoryViewModel { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await SearchViewModel.LoadPagedBudgetsAsync();
        await CategoryViewModel.LoadCategoriesAsync();
        await RecurringExpenseViewModel.LoadRecurringExpensesAsync();

        await base.OnInitializedAsync();
    }
}
