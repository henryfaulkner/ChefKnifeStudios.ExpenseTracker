using ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.AreYouSureDialogs;

public partial class DeleteExpenseAreYouSureDialog : ComponentBase, IDisposable
{
    [Inject] ILogger<DeleteRecurringExpenseAreYouSureDialog> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;
    [Inject] IToastService ToastService { get; set; } = null!;
    [Inject] IExpenseViewModel ExpenseViewModel { get; set; } = null!;

    readonly string[] _subscriptions = [
        nameof(IExpenseViewModel.SelectedExpense),
    ];

    protected override void OnInitialized()
    {
        ExpenseViewModel.PropertyChanged += ViewModel_OnPropertyChanged;
        base.OnInitialized();
    }

    public void Dispose()
    {
        ExpenseViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
        GC.SuppressFinalize(this);
    }

    void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_subscriptions.Contains(e.PropertyName) is false) return;
        Task.Run(async () => await InvokeAsync(StateHasChanged));
    }

    void HandleNoPressed()
    {
        Task.Run(async () =>
        {
            await Task.Delay(50);
            ExpenseViewModel.SelectedExpense = null;
        });
    }

    async Task HandleYesPressed()
    {
        await ExpenseViewModel.DeleteSelectedExpense();
    }

    void HandleOpenChanged(bool isOpen)
    {
        if (!isOpen) ExpenseViewModel.SelectedExpense = null;
    }

    void HandleDeleteExpensePressed(ExpenseDTO expense)
    {
        _ = ExpenseViewModel.DeleteSelectedExpense();
    }
}
