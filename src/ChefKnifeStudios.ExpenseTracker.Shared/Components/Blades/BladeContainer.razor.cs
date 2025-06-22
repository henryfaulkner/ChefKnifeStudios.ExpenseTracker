using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;

public partial class BladeContainer : ComponentBase, IDisposable
{
    [Parameter] public required RenderFragment ContentFragment { get; set; }

    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;
    [Inject] ICommonJsInteropService CommonJsInteropService { get; set; } = null!;

    bool _isOpen = false;
    Guid _uid = Guid.NewGuid();
    string _elementId => $"blade-{_uid.ToString()}";
    DateTime _lastOpenedUtc;
    const int MinOpenDurationMs = 300; 

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;
        await CommonJsInteropService.RegisterClickOutside(_elementId);
    }

    public void Dispose()
    {
        CommonJsInteropService.RemoveClickOutsideCallback(_uid);
    }

    public void Open()
    {
        _lastOpenedUtc = DateTime.UtcNow;
        _isOpen = true;
        CommonJsInteropService.AddClickOutsideCallback(HandleClosePressed, _uid);
        StateHasChanged();
    }

    public void Close()
    {
        // Prevent close if not enough time has passed since open
        if ((DateTime.UtcNow - _lastOpenedUtc).TotalMilliseconds < MinOpenDurationMs)
            return;

        _isOpen = false;
        CommonJsInteropService.RemoveClickOutsideCallback(_uid);
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
