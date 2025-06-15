using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Components.Pages;

public partial class Home : ComponentBase, IDisposable
{
    [Inject] ILogger<Home> Logger { get; set; } = null!;
    [Inject] ISearchViewModel SearchViewModel { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;

    bool _isGridVisible = true;

    protected override void OnInitialized()
    {
        SentrySdk.CaptureException(new Exception("Hello Sentry"));
        EventNotificationService.EventReceived += HandleEventReceived;

        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        await SearchViewModel.LoadPagedBudgetsAsync();

        await base.OnInitializedAsync();
    }

    public void Dispose()
    {
        EventNotificationService.EventReceived -= HandleEventReceived;
    }

    async Task HandleEventReceived(object sender, IEventArgs e)
    {
        switch (e)
        {
            case BudgetEventArgs budgetEvent:
            case ExpenseEventArgs expenseEvent:
                // HACK: this resolves a quickgrid disposed component issue
                //_isGridVisible = false;
                //StateHasChanged();
                await SearchViewModel.LoadPagedBudgetsAsync();
                //await Task.Delay(50);
                //_isGridVisible = true;
                //StateHasChanged();
                break;
            default:
                break;
        }
    }
}
