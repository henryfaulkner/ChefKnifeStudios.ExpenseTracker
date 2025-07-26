namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface ITaskQueueService
{
    void Enqueue(Func<Task> taskGenerator);
}
