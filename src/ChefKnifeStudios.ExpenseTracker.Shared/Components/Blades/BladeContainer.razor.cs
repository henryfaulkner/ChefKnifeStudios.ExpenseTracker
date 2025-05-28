using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;

public partial class BladeContainer : ComponentBase
{
    [Parameter] public required RenderFragment ContentFragment { get; set; }

    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;

    bool _isOpen = false;

    public void Open()
    {
        _isOpen = true;
        StateHasChanged();
    }

    public void Close()
    {
        _isOpen = false;
        StateHasChanged();
    }

    void HandleClosePressed()
    {
        EventNotificationService.PostEvent(
            this,
            new BladeEventArgs()
            { 
                Type = BladeEventArgs.Types.Close,
            }
        );
    }
}
