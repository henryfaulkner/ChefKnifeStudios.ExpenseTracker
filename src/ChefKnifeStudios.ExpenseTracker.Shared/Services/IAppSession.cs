using ChefKnifeStudios.ExpenseTracker.Shared.Models;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface IAppSessionService
{
    Task InitAsync(CancellationToken cancellationToken = default);
    Task SetSessionAsync(AppSession session, CancellationToken cancellationToken = default);
    Task<AppSession> GetSessionAsync(CancellationToken cancellationToken = default);
}