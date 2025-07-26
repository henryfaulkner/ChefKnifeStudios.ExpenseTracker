using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using System.Net;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components;

public partial class EditableExpenseCost : ComponentBase
{
    [Parameter] public required ExpenseDTO Expense { get; set; }
    [Parameter] public EventCallback StateHasChangedCallback { get; set; }

    [Inject] IStorageService StorageService { get; set; } = null!;
    [Inject] IToastService ToastService { get; set; } = null!;

    bool _isEditing = false;
    decimal? _cost;
    MatTextField<decimal?>? _textFieldCost;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _cost = Expense.Cost;
    }

    async Task HandleEditingStarted()
    {
        _isEditing = true;
        await _textFieldCost!.Ref.FocusAsync();
    }

    async Task HandleEditingStopped()
    {
        if (_cost == Expense.Cost)
        {
            _isEditing = false;
            return;
        }

        var res = await StorageService.UpdateExpenseCostAsync(Expense.Id, _cost ?? 0.0m);

        if (res.HttpStatusCode != HttpStatusCode.OK || res?.Data is null or false)
        {
            ToastService.ShowError("Expense cost failed to update");
            _cost = Expense.Cost;
            _isEditing = false;
            return;
        }

        ToastService.ShowSuccess("Expense cost updated");
        Expense.Cost = _cost ?? 0.0m;
        _isEditing = false;
        _ = StateHasChangedCallback.InvokeAsync();
    }
}
