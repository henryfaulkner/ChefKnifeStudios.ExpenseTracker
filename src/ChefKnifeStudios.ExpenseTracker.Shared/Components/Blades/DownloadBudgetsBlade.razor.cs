using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;

public partial class DownloadBudgetsBlade : ComponentBase, IDisposable
{
    [Inject] ILogger<DownloadBudgetsBlade> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;
    [Inject] IStorageService StorageService { get; set; } = null!;
    [Inject] IToastService ToastService { get; set; } = null!;
    [Inject] IExcelViewModel ExcelViewModel { get; set; } = null!;

    IEnumerable<BudgetDTO> _budgets = [];
    List<BudgetDTO> _selectedBudgets = [];
    BladeContainer? _bladeContainer;
    bool _keepBladeOpen = false;
    bool _isFormLoading = false;
    bool _isEmailLoading = false;
    bool _isEmailDialogOpen = false;
    string _emailRecipient = string.Empty;

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
            case BladeEventArgs { Type: BladeEventArgs.Types.DownloadBudgets } budgetBlade:
                var res = await StorageService.GetBudgetsAsync();
                if (!res.IsSuccess) ToastService.ShowWarning("Budgets failed to load.");
                _budgets = res.Data ?? [];
                _bladeContainer?.Open();
                break;
            case BladeEventArgs { Type: not BladeEventArgs.Types.DownloadBudgets }:
                _bladeContainer?.Close();
                break;
        }
        await Task.CompletedTask;
    }

    void HandleSelectedBudgetsValueChanged(bool isChecked, BudgetDTO checkboxBudget)
    {
        if (_selectedBudgets.Contains(checkboxBudget))
            _selectedBudgets.Remove(checkboxBudget);

        if (isChecked)
            _selectedBudgets.Add(checkboxBudget);
    }

    void HandleSubmitPressedAsync()
    {
        if (!_selectedBudgets.Any())
        {
            ToastService.ShowWarning("A budget is required");
            return;
        }

        _keepBladeOpen = true;
        _isFormLoading = true;
        _isEmailLoading = false;
        _isEmailDialogOpen = true;
    }

    async Task HandleEmailDialogOpenChanged(bool isOpen)
    {
        if (isOpen) return;
        _isFormLoading = false;
        _isEmailLoading = false;
        _isEmailDialogOpen = false;
        await Task.Delay(50);
        _keepBladeOpen = false;
    }

    async Task HandleEmailCancelPressed()
    {
        _isFormLoading = false;
        _isEmailLoading = false;
        _isEmailDialogOpen = false;
        await Task.Delay(50);
        _keepBladeOpen = false;
    }

    async Task HandleEmailSendPressed()
    {
        _isEmailLoading = true;
        try
        {
            await ExcelViewModel.SendEmailWithBudgetExcelAsync(
                _selectedBudgets,
                _emailRecipient
            );

            ToastService.ShowSuccess("Email sent successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Budget email could not be sent.");
            ToastService.ShowError("Email failed to send");
        }
        finally
        {
            _isFormLoading = false;
            _isEmailLoading = false;
            _isEmailDialogOpen = false;
            await Task.Delay(50);
            _keepBladeOpen = false;
        }
    }
}
