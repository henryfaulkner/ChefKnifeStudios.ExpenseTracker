using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;

public partial class ActiveRecurringExpensesBlade : ComponentBase
{
    [Inject] ILogger<ActiveRecurringExpensesBlade> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;
    [Inject] IToastService ToastService { get; set; } = null!;
    [Inject] IRecurringExpenseViewModel RecurringExpenseViewModel { get; set; } = null!;
    
    BladeContainer? _bladeContainer;

    readonly string[] _subscriptions = [];

    protected override void OnInitialized()
    {
        EventNotificationService.EventReceived += HandleEventReceived;
        RecurringExpenseViewModel.PropertyChanged += ViewModel_OnPropertyChanged;

        base.OnInitialized();
    }

    public void Dispose()
    {
        EventNotificationService.EventReceived -= HandleEventReceived;
        RecurringExpenseViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
    }

    void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_subscriptions.Contains(e.PropertyName) is false) return;
        Task.Run(async () => await InvokeAsync(StateHasChanged));
    }

    async Task HandleEventReceived(object sender, IEventArgs e)
    {
        switch (e)
        {
            case BladeEventArgs { Type: BladeEventArgs.Types.ActiveRecurringExpenses }:
                _bladeContainer?.Open();
                break;
            case BladeEventArgs { Type: not BladeEventArgs.Types.ActiveRecurringExpenses }:
                _bladeContainer?.Close();
                break;
            default:
                Logger.LogWarning("Event handler's switch statement fell through.");
                break;
        }
        await Task.CompletedTask;
    }
}
