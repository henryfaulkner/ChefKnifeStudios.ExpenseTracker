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
    [Inject] IToastService ToastService { get; set; } = null!;

    IEnumerable<BudgetDTO> _budgets = [];

    BladeContainer? _bladeContainer;
    BudgetDTO? _selectedBudget;
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
            case BladeEventArgs { Type: BladeEventArgs.Types.Budget } budgetBlade:
                var res = await StorageService.GetBudgetsAsync();
                if (!res.IsSuccess) ToastService.ShowWarning("Budgets failed to load.");
                _budgets = res.Data ?? [];
                if (budgetBlade.Data is BudgetDTO budget)
                {
                    _selectedBudget = budget;
                    _budget = budget.ExpenseBudget;
                }
                _bladeContainer?.Open();
                break;
            case BladeEventArgs { Type: not BladeEventArgs.Types.Budget }:
                _bladeContainer?.Close();
                break;
            default:
                Logger.LogWarning("Event handler's switch statement fell through.");
                break;
        }
        await Task.CompletedTask;
    }

    async Task HandleSubmitPressed(MouseEventArgs e)
    {
        if (_selectedBudget is null)
        {
            ToastService.ShowWarning("A budget is required");
            return;
        }

        if (!_budget.HasValue)
            return;
        
        _selectedBudget.ExpenseBudget = _budget.Value;

        var res = await StorageService.UpdateBudgetAsync(_selectedBudget);
        if (res.IsSuccess)
        {
            ToastService.ShowSuccess("Your budget updated");
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
        }
        else
        {
            ToastService.ShowError("Your budget failed to update");
        }
    }

    void HandleValueChanged(BudgetDTO budget)
    {
        _selectedBudget = budget;
        _budget = budget.ExpenseBudget;
    }

    void Clear()
    {
        _selectedBudget = null;
        _budget = null;
    }
}
