using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;

public partial class BudgetBlade : ComponentBase, IDisposable
{
    [Inject] ILogger<BudgetBlade> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;
    [Inject] IStorageService StorageService { get; set; } = null!;

    BladeContainer? _bladeContainer;
    string? _name; 
    DateTime _startDate = DateTime.Now;
    DateTime _endDate = DateTime.Now.AddMonths(1);
    decimal? _budget;

    protected override void OnInitialized()
    {
        EventNotificationService.EventReceived += HandleEventReceived;

        base.OnInitialized();
    }

    public void Dispose()
    {
        EventNotificationService.EventReceived -= HandleEventReceived;
    }

    async Task HandleEventReceived(object sender, IEventArgs e)
    {
        switch (e)
        {
            case BladeEventArgs { Type: BladeEventArgs.Types.Budget}:
                _bladeContainer?.Open();
                StateHasChanged();
                break;
            case BladeEventArgs { Type: BladeEventArgs.Types.Close or BladeEventArgs.Types.Expense }:
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
        if (_name is null || !_budget.HasValue)
            return;

        BudgetDTO budget = new()
        {
            Name = _name,
            StartDate = _startDate,
            EndDate = _endDate,
            ExpenseBudget = _budget.Value,
        };

        Task.Run(async () =>
        {
            await StorageService.AddBudgetAsync(budget);
            EventNotificationService.PostEvent(
                this,
                new BudgetEventArgs()
                {
                    Type = BudgetEventArgs.Types.Added,
                }
            );
            EventNotificationService.PostEvent(
                this,
                new BladeEventArgs()
                {
                    Type = BladeEventArgs.Types.Close,
                }
            );
            Clear();
        });
    }

    void Clear()
    {
        _name = null; 
        _startDate = DateTime.Now;
        _endDate = DateTime.Now.AddMonths(1);
        _budget = null;
    }
}
