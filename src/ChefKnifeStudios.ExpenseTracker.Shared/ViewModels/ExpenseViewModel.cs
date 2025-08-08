using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface IExpenseViewModel : IViewModel
{
    bool IsListening { get; }
    Task<Receipt?> PickPhotoForReceiptAsync();
    Task<Receipt?> CapturePhotoForReceiptAsync();
    Task StartListeningForExpenseAsync();
    Task<TextToExpenseResponseDTO?> StopListeningForExpenseAsync();
}

public class ExpenseViewModel : BaseViewModel, IExpenseViewModel
{
    readonly IStorageService _storageService;
    readonly ICameraService _cameraService;
    readonly ISemanticService _semanticService;
    readonly IMicrophoneService _microphoneService;
    readonly IEventNotificationService _eventNotificationService;
    readonly ILogger<ExpenseViewModel> _logger;

    bool _isListening = false;
    public bool IsListening
    {
        get => _isListening;
        private set => SetValue(ref _isListening, value);
    }

    public ExpenseViewModel(
        IStorageService storageService,
        ICameraService cameraService,
        ISemanticService semanticService,
        IMicrophoneService microphoneService,
        IEventNotificationService eventNotificationService,
        ILogger<ExpenseViewModel> logger)
    {
        _storageService = storageService;
        _cameraService = cameraService;
        _semanticService = semanticService;
        _microphoneService = microphoneService;
        _eventNotificationService = eventNotificationService;
        _logger = logger;
    }

    public async Task<Receipt?> PickPhotoForReceiptAsync()
    {
        try
        {
            PhotoResult? photo = await _cameraService.PickPhotoAsync();
            if (photo?.FileStream is null) return null;

            using var memoryStream = new MemoryStream();
            await photo.FileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var response = await _semanticService.ScanReceiptAsync(memoryStream);
            if (response.HttpStatusCode != HttpStatusCode.OK || response?.Data?.FirstOrDefault() is null)
            {
                _logger.LogError("Scanning API failed");
                return null;
            }
            var receiptDTO = response.Data.First();

            var response2 = await _semanticService.LabelReceiptDetailsAsync(receiptDTO);
            if (response2.HttpStatusCode != HttpStatusCode.OK || response2?.Data?.Labels is null)
            {
                _logger.LogError("Labeling API failed");
                return null;
            }
            var labelsDTO = response2.Data;
            _eventNotificationService.PostEvent(
                this,
                new CategoryEventArgs()
                {
                    Type = CategoryEventArgs.EventTypes.AddExpenseCategories,
                    Data = new CategoryEventArgs.EventData() { Categories = labelsDTO.Categories ?? [], },
                }
            );

            return new Receipt(receiptDTO, labelsDTO);
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "An error occurred");
            return null;
        }
    }

    public async Task<Receipt?> CapturePhotoForReceiptAsync()
    {
        try
        { 
            PhotoResult? photo = await _cameraService.CapturePhotoAsync();
            if (photo?.FileStream is null) return null;

            using var memoryStream = new MemoryStream();
            await photo.FileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var response = await _semanticService.ScanReceiptAsync(memoryStream);
            if (response.HttpStatusCode != HttpStatusCode.OK || response?.Data?.FirstOrDefault() is null)
            {
                _logger.LogError("Scanning API failed");
                return null;
            }
            var receiptDTO = response.Data.First();

            var response2 = await _semanticService.LabelReceiptDetailsAsync(receiptDTO);
            if (response2.HttpStatusCode != HttpStatusCode.OK || response2?.Data?.Labels is null)
            {
                _logger.LogError("Labeling API failed");
                return null;
            }
            var labelsDTO = response2.Data;
            _eventNotificationService.PostEvent(
                this,
                new CategoryEventArgs() 
                { 
                    Type = CategoryEventArgs.EventTypes.AddExpenseCategories, 
                    Data = new CategoryEventArgs.EventData() { Categories = labelsDTO.Categories ?? [], },
                }
            );

            return new Receipt(receiptDTO, labelsDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
            return null;
        }
    }

    public async Task StartListeningForExpenseAsync()
    {
        await _microphoneService.StartListeningAsync();
        IsListening = true;
    }

    public async Task<TextToExpenseResponseDTO?> StopListeningForExpenseAsync()
    {
        TextToExpenseResponseDTO? result = null;
        string recognizedText = await _microphoneService.StopListeningAsync();
        IsListening = false;
        TextToExpenseRequestDTO req = new() { Text = recognizedText, };
        var response = await _semanticService.TextToExpenseAsync(req);
        if (response.HttpStatusCode != HttpStatusCode.OK || response?.Data is null)
        {
            _logger.LogError("TextToExpense API failed");
            return result;
        }
        result = response.Data;
        _eventNotificationService.PostEvent(
            this,
            new CategoryEventArgs()
            {
                Type = CategoryEventArgs.EventTypes.AddExpenseCategories,
                Data = new CategoryEventArgs.EventData() { Categories = result.Categories ?? [], },
            }
        );

        return result;
    }
}
