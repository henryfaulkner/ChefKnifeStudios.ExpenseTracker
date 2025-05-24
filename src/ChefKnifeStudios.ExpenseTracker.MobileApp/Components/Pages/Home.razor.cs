using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] ISearchViewModel SearchViewModel { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await SearchViewModel.LoadPagedBudgets();

        await base.OnInitializedAsync();
    }
}
