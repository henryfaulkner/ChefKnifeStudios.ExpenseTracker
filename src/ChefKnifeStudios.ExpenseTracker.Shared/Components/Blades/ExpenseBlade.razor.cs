﻿using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;

public partial class ExpenseBlade : ComponentBase, IDisposable
{
    [Inject] ILogger<BudgetBlade> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;
    [Inject] IStorageService StorageService { get; set; } = null!;
    [Inject] IApiService ApiService { get; set; } = null!;
    [Inject] IReceiptViewModel ReceiptViewModel { get; set; } = null!;

    BladeContainer? _bladeContainer;
    IEnumerable<BudgetDTO> _budgets = [];

    string? _name;
    decimal? _cost;
    BudgetDTO? _selectedBudget;
    List<string>? _labels;

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
            case BladeEventArgs { Type: BladeEventArgs.Types.Expense }:
                _budgets = await StorageService.GetBudgetsAsync();
                _bladeContainer?.Open();
                break;
            case BladeEventArgs { Type: BladeEventArgs.Types.Close or BladeEventArgs.Types.Budget }:
                _bladeContainer?.Close();
                break;
            default:
                Logger.LogWarning("Event handler's switch statement fell through.");
                break;
        }
        await Task.CompletedTask;
    }

    async Task HandleSubmitPressedAsync(MouseEventArgs e)
    {
        if (_name is null || !_cost.HasValue || _selectedBudget is null)
            return;

        ExpenseDTO expense = new()
        {
            Name = _name,
            Cost = _cost.Value,
            BudgetId = _selectedBudget.Id,
            Labels = _labels ?? [],
        };

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
        Clear();
    }

    void Clear()
    {
        _name = null;
        _cost = null;
    }

    async Task HandlePickPicture()
    {
        var receipt = await ReceiptViewModel.PickPhotoForReceiptAsync();
        if (receipt == null) throw new ApplicationException("Receipt returned null");
        if (!string.IsNullOrWhiteSpace(receipt.MerchantName)) receipt.Labels.Add(receipt.MerchantName);

        _name = receipt.Name;
        _cost = receipt.Total;
        _labels = receipt.Labels;
    }

    async Task HandleTakePicture()
    {
        var receipt = await ReceiptViewModel.CapturePhotoForReceiptAsync();
        if (receipt == null) throw new ApplicationException("Receipt returned null");
        if (!string.IsNullOrWhiteSpace(receipt.MerchantName)) receipt.Labels.Add(receipt.MerchantName);

        _name = receipt.Name;
        _cost = receipt.Total;
        _labels = receipt.Labels;
    }
}
