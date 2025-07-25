using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface IRecurringExpenseViewModel : IViewModel
{
    Task<IEnumerable<RecurringExpenseConfigDTO>> GetRecurringExpensesAsync();
    Task<bool> DeleteRecurringExpense(int recurringExpenseId);
}

public class RecurringExpenseViewModel : BaseViewModel, IRecurringExpenseViewModel
{
    readonly ILogger<RecurringExpenseViewModel> _logger;
    readonly IToastService _toastService;
    readonly IStorageService _storageService;

    public RecurringExpenseViewModel(
        ILogger<RecurringExpenseViewModel> logger,
        IToastService toastService,
        IStorageService storageService)
    {
        _logger = logger;
        _toastService = toastService;
        _storageService = storageService;
    }

    public async Task<IEnumerable<RecurringExpenseConfigDTO>> GetRecurringExpensesAsync()
    {
        try
        {
            var res = await _storageService.GetRecurringExpensesAsync();
            if (res.HttpStatusCode != HttpStatusCode.OK || res?.Data is null)
            {
                _logger.LogError("Recurring Expense API failed");
                _toastService.ShowError("Recurring expenses could not be loaded");
                return [];
            }
            return res.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
            _toastService.ShowError("Recurring expenses could not be loaded");
            return [];
        }
    }

    public async Task<bool> DeleteRecurringExpense(int recurringExpenseId)
    {
        try
        {
            var res = await _storageService.DeleteRecurringExpenseAsync(recurringExpenseId);
            if (res.HttpStatusCode != HttpStatusCode.OK || res is null || res.Data == false)
            {
                _logger.LogError("Recurring Expense API failed");
                _toastService.ShowError("Recurring expense could not be deleted");
                return false;
            }
            return res.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
            _toastService.ShowError("Recurring expense could not be deleted");
            return false;
        }
    }
}
