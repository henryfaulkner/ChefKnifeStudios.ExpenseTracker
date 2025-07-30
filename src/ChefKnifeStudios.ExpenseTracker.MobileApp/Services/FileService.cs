using ChefKnifeStudios.ExpenseTracker.Shared.Services;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class FileService : IFileService
{
    public string GetAppDataFolder() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public string GetCacheFolder() => FileSystem.CacheDirectory;
}
