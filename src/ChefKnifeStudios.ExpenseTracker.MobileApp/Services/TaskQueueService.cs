using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class TaskQueueService : ITaskQueueService
{
    readonly ConcurrentQueue<Func<Task>> _tasks = new();
    readonly SemaphoreSlim _semaphore = new(1, 1);
    readonly ILogger<TaskQueueService> _logger;
    bool _isProcessing;

    public TaskQueueService(ILogger<TaskQueueService> logger)
    {
        _logger = logger;
    }

    public void Enqueue(Func<Task> taskGenerator)
    {
        _tasks.Enqueue(taskGenerator);
        _ = ProcessQueueAsync();
    }

    async Task ProcessQueueAsync()
    {
        if (_isProcessing) return;
        _isProcessing = true;

        while (_tasks.TryDequeue(out var task))
        {
            await _semaphore.WaitAsync();
            try
            {
                await task();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during task execution.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        _isProcessing = false;
    }
}