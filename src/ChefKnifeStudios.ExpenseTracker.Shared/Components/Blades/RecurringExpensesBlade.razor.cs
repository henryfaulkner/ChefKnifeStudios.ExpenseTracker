using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;

public partial class RecurringExpensesBlade : ComponentBase
{
    [CascadingParameter] public IRecurringExpenseViewModel RecurringExpenseViewModel { get; set; } = null!;

    [Inject] ILogger<RecurringExpensesBlade> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;
    [Inject] IToastService ToastService { get; set; } = null!;
    
    BladeContainer? _bladeContainer;
    bool _isDialogOpen = false;
    RecurringExpenseConfigDTO? _selectedRecurringExpense = null;
    bool _keepBladeOpen => RecurringExpenseViewModel.SelectedRecurringExpenseForDeletion is { };
    

    readonly string[] _subscriptions = [
        nameof(IRecurringExpenseViewModel.RecurringExpenses),
        nameof(IRecurringExpenseViewModel.SelectedRecurringExpenseForDeletion),
    ];

    protected override void OnInitialized()
    {
        EventNotificationService.EventReceived += HandleEventReceived;
        RecurringExpenseViewModel.PropertyChanged += ViewModel_OnPropertyChanged;

        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await RecurringExpenseViewModel.LoadRecurringExpensesAsync();
    }

    public void Dispose()
    {
        EventNotificationService.EventReceived -= HandleEventReceived;
        RecurringExpenseViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
        GC.SuppressFinalize(this);
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
        }
        await Task.CompletedTask;
    }

    void HandleDeletePressed(RecurringExpenseConfigDTO recurringExpense)
    {
        RecurringExpenseViewModel.SelectedRecurringExpenseForDeletion = recurringExpense;
    }
}
