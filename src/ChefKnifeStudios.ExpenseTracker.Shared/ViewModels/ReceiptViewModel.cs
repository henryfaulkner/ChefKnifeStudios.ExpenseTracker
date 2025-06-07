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

public interface IReceiptViewModel : IViewModel
{
    Task<Receipt?> PickPhotoForReceiptAsync();
    Task<Receipt?> CapturePhotoForReceiptAsync();
}

public class ReceiptViewModel : BaseViewModel, IReceiptViewModel
{
    readonly IStorageService _storageService;
    readonly ICameraService _cameraService;
    readonly IApiService _apiService;
    readonly ILogger<ReceiptViewModel> _logger;

    public ReceiptViewModel(IStorageService storageService,
        ICameraService cameraService,
        IApiService apiService,
        ILogger<ReceiptViewModel> logger)
    {
        _storageService = storageService;
        _cameraService = cameraService;
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<Receipt?> PickPhotoForReceiptAsync()
    {
        PhotoResult? photo = await _cameraService.PickPhotoAsync();
        if (photo?.FileStream is null) return null;

        var response = await _apiService.ScanReceiptAsync(photo.FileStream);
        if (response.HttpStatusCode != HttpStatusCode.OK || response?.Data?.FirstOrDefault() is null)
        {
            _logger.LogError("Scanning API failed");
            return null;
        }
        var receiptDTO = response.Data.First();

        var response2 = await _apiService.LabelReceiptDetailsAsync(receiptDTO);
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

        var response = await _apiService.ScanReceiptAsync(photo.FileStream);
        if (response.HttpStatusCode != HttpStatusCode.OK || response?.Data?.FirstOrDefault() is null)
        {
            _logger.LogError("Scanning API failed");
            return null;
        }
        var receiptDTO = response.Data.First();

        var response2 = await _apiService.LabelReceiptDetailsAsync(receiptDTO);
        if (response2.HttpStatusCode != HttpStatusCode.OK || response2?.Data?.Labels is null)
        {
            _logger.LogError("Labeling API failed");
            return null;
        }
        var labelsDTO = response2.Data;

        return new Receipt(receiptDTO, labelsDTO);
    }
}
