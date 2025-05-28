using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;

public partial class BudgetBlade : ComponentBase
{
    [Inject] ILogger<BudgetBlade> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;

    BladeContainer? _bladeContainer;
    DateTime _startDate = DateTime.Now;
    DateTime _endDate = DateTime.Now.AddMonths(1);
    decimal? _budget;

    protected override void OnInitialized()
    {
        EventNotificationService.EventReceived += HandleEventReceived;

        base.OnInitialized();
    }

    async Task HandleEventReceived(object sender, IEventArgs e)
    {
        switch (e)
        {
            case BladeEventArgs { Type: BladeEventArgs.Types.Budget}:
                _bladeContainer?.Open();
                StateHasChanged();
                break;
            case BladeEventArgs { Type: BladeEventArgs.Types.Close or BladeEventArgs.Types.Expense }:
                _bladeContainer?.Close();
                StateHasChanged();
                break;
            default:
                Logger.LogWarning("Event handler's switch statement fell through.");
                break;
        }
        await Task.CompletedTask;
    }

    void HandleSubmitPressed(MouseEventArgs e)
    {
        return;
    }
}
