namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface IFileService
{
    string GetAppDataFolder();
    string GetCacheFolder();
}
