using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.RecurringExpenseFunctionApp;

public class RecurringExpenseFunction
{
    private readonly ILogger _logger;

    public RecurringExpenseFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RecurringExpenseFunction>();
    }

    [Function("Function1")]
    public void Run([TimerTrigger("0 0 0 1 * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}