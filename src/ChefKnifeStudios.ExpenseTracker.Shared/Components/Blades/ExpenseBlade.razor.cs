using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;

public partial class ExpenseBlade : ComponentBase, IDisposable
{
    [Inject] ILogger<BudgetBlade> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;
    [Inject] IStorageService StorageService { get; set; } = null!;
    [Inject] ISemanticService SemanticService { get; set; } = null!;
    [Inject] IToastService ToastService { get; set; } = null!;
    [Inject] IExpenseViewModel ExpenseViewModel { get; set; } = null!;
    [Inject] IMicrophoneService MicrophoneService { get; set; } = null!;

    BladeContainer? _bladeContainer;
    IEnumerable<BudgetDTO> _budgets = [];

    string? _name;
    decimal? _cost;
    BudgetDTO? _selectedBudget;
    List<string>? _labels;
    bool _isLoading = false;

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
                var res = await StorageService.GetBudgetsAsync();
                if (!res.IsSuccess) ToastService.ShowWarning("Budgets failed to load.");
                _budgets = res.Data ?? [];
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

        _isLoading = true;
        ReceiptLabelsDTO receiptLabels = new()
        {
            Name = _name,
            Labels = _labels,
        };

        var embeddingRes = await SemanticService.CreateSemanticEmbeddingAsync(receiptLabels);
        var embedding = embeddingRes.Data;
        if (embedding is not SemanticEmbeddingDTO) throw new ApplicationException("Embedding data is null.");

        ExpenseDTO expense = new()
        {
            Name = _name,
            Cost = _cost.Value,
            BudgetId = _selectedBudget.Id,
            Labels = _labels ?? [],
            ExpenseSemantic = new()
            {
                Labels = embedding.Labels,
                SemanticEmbedding = embedding.Embedding,
            }
        };

        await StorageService.AddExpenseAsync(expense);
        await SemanticService.UpsertExpenseAsync(expense);
        _isLoading = false;
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

    async Task HandlePickPictureAsync()
    {
        _isLoading = true;
        var receipt = await ExpenseViewModel.PickPhotoForReceiptAsync();
        if (receipt == null) throw new ApplicationException("Receipt returned null");
        if (!string.IsNullOrWhiteSpace(receipt.MerchantName)) receipt.Labels.Add(receipt.MerchantName);

        _name = receipt.Name;
        _cost = receipt.Total;
        _labels = receipt.Labels;
        _isLoading = false;
    }

    async Task HandleTakePictureAsync()
    {
        _isLoading = true;
        var receipt = await ExpenseViewModel.CapturePhotoForReceiptAsync();
        if (receipt == null) throw new ApplicationException("Receipt returned null");
        if (!string.IsNullOrWhiteSpace(receipt.MerchantName)) receipt.Labels.Add(receipt.MerchantName);

        _name = receipt.Name;
        _cost = receipt.Total;
        _labels = receipt.Labels;
        _isLoading = false;
    }

    async Task HandleStartListeningAsync()
    {
        _isLoading = true;
        await ExpenseViewModel.StartListeningForExpenseAsync();
    }

    async Task HandleStopListeningAsync()
    {
        var data = await ExpenseViewModel.StopListeningForExpenseAsync();
        if (data is null)
        {
            ToastService.ShowWarning("Expense could not be created from description.");
            return;
        }

        _name = data.Name;
        _cost = data.Price;
        _labels = data.Labels.ToList();

        _isLoading = false;
    }
}
