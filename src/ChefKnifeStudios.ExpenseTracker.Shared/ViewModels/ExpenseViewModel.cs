using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface IExpenseViewModel : IViewModel
{
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
    readonly ILogger<ExpenseViewModel> _logger;

    public ExpenseViewModel(IStorageService storageService,
        ICameraService cameraService,
        ISemanticService semanticService,
        IMicrophoneService microphoneService,
        ILogger<ExpenseViewModel> logger)
    {
        _storageService = storageService;
        _cameraService = cameraService;
        _semanticService = semanticService;
        _microphoneService = microphoneService;
        _logger = logger;
    }

    public async Task<Receipt?> PickPhotoForReceiptAsync()
    {
        PhotoResult? photo = await _cameraService.PickPhotoAsync();
        if (photo?.FileStream is null) return null;

        var response = await _semanticService.ScanReceiptAsync(photo.FileStream);
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

        return new Receipt(receiptDTO, labelsDTO);
    }

    public async Task<Receipt?> CapturePhotoForReceiptAsync()
    {
        PhotoResult? photo = await _cameraService.CapturePhotoAsync();
        if (photo?.FileStream is null) return null;

        var response = await _semanticService.ScanReceiptAsync(photo.FileStream);
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

        return new Receipt(receiptDTO, labelsDTO);
    }

    public async Task StartListeningForExpenseAsync()
    {
        await _microphoneService.StartListeningAsync();
    }

    public async Task<TextToExpenseResponseDTO?> StopListeningForExpenseAsync()
    {
        TextToExpenseResponseDTO? result = null;
        string recognizedText = await _microphoneService.StopListeningAsync();
        TextToExpenseRequestDTO req = new() { Text = recognizedText, };
        var response = await _semanticService.TextToExpenseAsync(req);
        if (response.HttpStatusCode != HttpStatusCode.OK || response?.Data is null)
        {
            _logger.LogError("TextToExpense API failed");
            return result;
        }
        result = response.Data;

        return result;
    }
}
