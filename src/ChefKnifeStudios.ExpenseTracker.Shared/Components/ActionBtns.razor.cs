using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components;

public partial class ActionBtns : ComponentBase
{
    [Inject] ILogger<ActionBtns> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;

    enum ActionStates
    {
        Closed,
        Open,
    }
    ActionStates _actionState = ActionStates.Closed;

    void HandleOpenPressed()
    {
        _actionState = ActionStates.Open;       
    }

    void HandleClosePressed()
    {
        _actionState = ActionStates.Closed;
    }

    void HandleBudgetPressed()
    {
        EventNotificationService.PostEvent(
            this,
            new BladeEventArgs()
            {
                Type = BladeEventArgs.Types.Budget,
            }
        );
    }

    void HandleExpensePressed()
    {
        EventNotificationService.PostEvent(
            this,
            new BladeEventArgs()
            {
                Type = BladeEventArgs.Types.Expense,
            }
        );
    }
}
