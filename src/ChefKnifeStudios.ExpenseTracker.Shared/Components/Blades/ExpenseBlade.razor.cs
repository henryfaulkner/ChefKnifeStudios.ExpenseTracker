using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;

public partial class ExpenseBlade : ComponentBase
{
    [Inject] ILogger<BudgetBlade> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;
    [Inject] IStorageService StorageService { get; set; } = null!;

    BladeContainer? _bladeContainer;

    string? _name;
    decimal? _cost;

    protected override void OnInitialized()
    {
        EventNotificationService.EventReceived += HandleEventReceived;

        base.OnInitialized();
    }

    async Task HandleEventReceived(object sender, IEventArgs e)
    {
        switch (e)
        {
            case BladeEventArgs { Type: BladeEventArgs.Types.Expense }:
                _bladeContainer?.Open();
                StateHasChanged();
                break;
            case BladeEventArgs { Type: BladeEventArgs.Types.Close or BladeEventArgs.Types.Budget }:
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
        if (_name is null || !_cost.HasValue)
            return;

        ExpenseDTO expense = new()
        {
            Name = _name,
            Cost = _cost.Value,
        };

        Task.Run(async () =>
        {
            await StorageService.AddExpenseAsync(expense);
            EventNotificationService.PostEvent(
                this,
                new ExpenseEventArgs()
                { 
                    Type = ExpenseEventArgs.Types.Added,
                }
            );
            EventNotificationService.PostEvent(
                this,
                new BladeEventArgs()
                {
                    Type = BladeEventArgs.Types.Close,
                }
            );
        });
    }
}
