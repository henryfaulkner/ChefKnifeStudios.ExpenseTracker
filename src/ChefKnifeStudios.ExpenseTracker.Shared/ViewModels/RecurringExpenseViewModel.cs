using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface IRecurringExpenseViewModel : IViewModel
{
    List<RecurringExpenseConfigDTO> RecurringExpenses { get; }
    RecurringExpenseConfigDTO? SelectedRecurringExpenseForDeletion { get; set; }

    Task LoadRecurringExpensesAsync();
    Task<bool> DeleteRecurringExpenseAsync(int recurringExpenseId);
}

public class RecurringExpenseViewModel : BaseViewModel, IRecurringExpenseViewModel, IDisposable
{
    readonly ILogger<RecurringExpenseViewModel> _logger;
    readonly IToastService _toastService;
    readonly IStorageService _storageService;
    readonly IEventNotificationService _eventNotificationService;

    List<RecurringExpenseConfigDTO> _recurringExpenses = [];
    public List<RecurringExpenseConfigDTO> RecurringExpenses
    {
        get => _recurringExpenses;
        private set => SetValue(ref _recurringExpenses, value);
    }

    RecurringExpenseConfigDTO? _selectedRecurringExpenseForDeletion = null;
    public RecurringExpenseConfigDTO? SelectedRecurringExpenseForDeletion
    {
        get => _selectedRecurringExpenseForDeletion;
        set => SetValue(ref _selectedRecurringExpenseForDeletion, value);
    }

    public RecurringExpenseViewModel(
        ILogger<RecurringExpenseViewModel> logger,
        IToastService toastService,
        IStorageService storageService,
        IEventNotificationService eventNotificationService)
    {
        _logger = logger;
        _toastService = toastService;
        _storageService = storageService;
        _eventNotificationService = eventNotificationService;
        _eventNotificationService.EventReceived += HandleEventReceived;
    }

    public void Dispose()
    {
        _eventNotificationService.EventReceived -= HandleEventReceived;
        GC.SuppressFinalize(this);
    }

    public async Task LoadRecurringExpensesAsync()
    {
        try
        {
            var res = await _storageService.GetRecurringExpensesAsync();
            if (res.HttpStatusCode != HttpStatusCode.OK || res?.Data is null)
            {
                _logger.LogError("Recurring Expense API failed");
                _toastService.ShowError("Recurring expenses could not be loaded");
                RecurringExpenses = [];
                return;
            }
            RecurringExpenses = res?.Data?.ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
            _toastService.ShowError("Recurring expenses could not be loaded");
            RecurringExpenses = [];
        }
    }

    public async Task<bool> DeleteRecurringExpenseAsync(int recurringExpenseId)
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
            await LoadRecurringExpensesAsync();
            return res.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
            _toastService.ShowError("Recurring expense could not be deleted");
            return false;
        }
    }

    async Task HandleEventReceived(object sender, IEventArgs args)
    {
        if (args is RecurringExpenseEventArgs recurringExpenseEventArgs)
        {
            await LoadRecurringExpensesAsync();
        }
    }
}
