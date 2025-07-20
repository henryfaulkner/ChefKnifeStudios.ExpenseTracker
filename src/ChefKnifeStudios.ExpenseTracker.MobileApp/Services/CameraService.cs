using ChefKnifeStudios.ExpenseTracker.Shared.Models;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class CameraService : ICameraService
{
    readonly ILogger<CameraService> _logger;

    public CameraService(ILogger<CameraService> logger)
    {
        _logger = logger;
    }

    public async Task<PhotoResult?> PickPhotoAsync()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult? file = await MediaPicker.Default.PickPhotoAsync();
            if (file == null)
            {
                _logger.LogInformation("Photo not able to be picked");
                return null;
            }
            return new PhotoResult()
            {
                FileName = file.FileName,
                FilePath = file.FullPath,
                ContentType = file.ContentType,
                FileStream = await file.OpenReadAsync(),
            };
        }
        throw new NotSupportedException("Photo picker is not supported");
    }

    public async Task<PhotoResult?> CapturePhotoAsync()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            ((MainPage)Application.Current.MainPage).SetCameraActive(true);
            FileResult? file = await MediaPicker.Default.CapturePhotoAsync();
            ((MainPage)Application.Current.MainPage).SetCameraActive(false);
            if (file == null)
            {
                _logger.LogInformation("Photo not able to be captured");
                return null;
            }
            return new PhotoResult()
            {
                FileName = file.FileName,
                FilePath = file.FullPath,
                ContentType = file.ContentType,
                FileStream = await file.OpenReadAsync(),
            };
        }
        throw new NotSupportedException("Camera capture is not supported");
    }
}
