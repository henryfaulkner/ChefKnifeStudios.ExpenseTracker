using Microsoft.AspNetCore.Components;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.AreYouSureDialogs;

public partial class AreYouSureDialog : ComponentBase
{
    [Parameter] public EventCallback NoCallback { get; set; }
    [Parameter] public EventCallback YesCallback { get; set; }
    [Parameter] public RenderFragment? HeaderContent { get; set; }
    [Parameter] public RenderFragment? BodyContent { get; set; }

    async Task HandleNoPressed() => await NoCallback.InvokeAsync();
    async Task HandleYesPressed() => await YesCallback.InvokeAsync();
}
