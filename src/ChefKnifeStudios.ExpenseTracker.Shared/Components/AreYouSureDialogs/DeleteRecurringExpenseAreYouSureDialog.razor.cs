using ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.AreYouSureDialogs;

public partial class DeleteRecurringExpenseAreYouSureDialog : ComponentBase, IDisposable
{
    [CascadingParameter] public IRecurringExpenseViewModel RecurringExpenseViewModel { get; set; } = null!;

    [Inject] ILogger<DeleteRecurringExpenseAreYouSureDialog> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;
    [Inject] IToastService ToastService { get; set; } = null!;

    readonly string[] _subscriptions = [
        nameof(IRecurringExpenseViewModel.SelectedRecurringExpenseForDeletion),
    ];

    protected override void OnInitialized()
    {
        RecurringExpenseViewModel.PropertyChanged += ViewModel_OnPropertyChanged;
        base.OnInitialized();
    }

    public void Dispose()
    {
        RecurringExpenseViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
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
            RecurringExpenseViewModel.SelectedRecurringExpenseForDeletion = null;
        });
    }

    async Task HandleYesPressed()
    {
        if (RecurringExpenseViewModel.SelectedRecurringExpenseForDeletion == null)
        {
            ToastService.ShowError("Recurring expense could not be canceled");
            Logger.LogError("Recurring expense dialog had a null recurring expense when handling YES action.");
            return;
        }
        await RecurringExpenseViewModel.DeleteRecurringExpenseAsync(
             RecurringExpenseViewModel.SelectedRecurringExpenseForDeletion.Id
        );
        RecurringExpenseViewModel.SelectedRecurringExpenseForDeletion = null;
    }

    void HandleOpenChanged(bool isOpen)
    {
        if (!isOpen) RecurringExpenseViewModel.SelectedRecurringExpenseForDeletion = null;
    }
}
