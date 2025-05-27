using Microsoft.AspNetCore.Components;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components;

public partial class ActionBtns : ComponentBase
{
    enum ActionStates
    {
        Closed,
        Open,
    }
    ActionStates _actionState = ActionStates.Closed;

    void HandleOpen()
    {
        _actionState = ActionStates.Open;       
    }

    void HandleClose()
    {
        _actionState = ActionStates.Closed;
    }
}
