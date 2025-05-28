using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] ILogger<Home> Logger { get; set; } = null!;
    [Inject] ISearchViewModel SearchViewModel { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await SearchViewModel.LoadPagedBudgets();

        await base.OnInitializedAsync();
    }
}
