namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface IMicrophoneService
{
    Task StartListeningAsync(CancellationToken cancellationToken = default);
    string GetCurrentText();
    Task<string> StopListeningAsync(CancellationToken cancellationToken = default);
}
