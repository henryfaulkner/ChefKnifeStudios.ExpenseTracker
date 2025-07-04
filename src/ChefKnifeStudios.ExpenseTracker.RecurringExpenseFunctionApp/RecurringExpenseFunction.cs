using ChefKnifeStudios.ExpenseTracker.BL.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.RecurringExpenseFunctionApp;

public class RecurringExpenseFunction
{
    readonly ILogger _logger;
    readonly IStorageService _storageService;

    public RecurringExpenseFunction(
        ILoggerFactory loggerFactory,
        IStorageService storageService)
    {
        _logger = loggerFactory.CreateLogger<RecurringExpenseFunction>();
        _storageService = storageService;
    }

    [Function("RecurringExpenseFunction")]
    public async Task Run([TimerTrigger("10 * * * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        try
        {
            _logger.LogInformation("Start RecurringExpenseFunction");
            _logger.LogInformation("Calling ProcessRecurringExpensesAsync");
            await _storageService.ProcessRecurringExpensesAsync();
            _logger.LogInformation("Successfully completed ProcessRecurringExpensesAsync");
            _logger.LogInformation("End RecurringExpenseFunction");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred: {ex.Message} - Inner: {ex.InnerException?.Message}");
            throw;
        }
    }
}