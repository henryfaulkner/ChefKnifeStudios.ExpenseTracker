using ChefKnifeStudios.ExpenseTracker.Shared.Models;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface ICameraService
{
    Task<PhotoResult?> PickPhotoAsync();
    Task<PhotoResult?> CapturePhotoAsync();
}
