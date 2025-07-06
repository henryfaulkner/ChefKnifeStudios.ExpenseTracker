using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Net;
using System.Net.Http.Json;
using ChefKnifeStudios.ExpenseTracker.Shared.Models;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class SemanticService : ISemanticService
{
    readonly HttpClient _httpClient;
    readonly ILogger<SemanticService> _logger;
    readonly string _baseUrl = string.Empty;

    public SemanticService(
        IHttpClientFactory httpClientFactory,
        ILogger<SemanticService> logger, 
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("ExpenseTrackerAPI");
        _logger = logger;
        _baseUrl = configuration.GetValue<string>("ApiBaseUrl") ?? string.Empty;
        _baseUrl += "/semantic";
    }

    public async Task<ApiResponse<IEnumerable<ReceiptDTO>?>> ScanReceiptAsync(Stream fileStream)
    {
        try
        { 
            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(fileContent, "file", "receipt.jpg");

            var response = await _httpClient.PostAsync($"{_baseUrl}/scan-receipt", form);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("scan-receipt endpoint failed.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<IEnumerable<ReceiptDTO>?>(content, JsonOptions.Get());

            return new ApiResponse<IEnumerable<ReceiptDTO>?>(obj, response.StatusCode);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<ReceiptDTO>?>()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Data = null,
            };
        }
    }

    public async Task<ApiResponse<TextToExpenseResponseDTO?>> TextToExpenseAsync(TextToExpenseRequestDTO request)
    {
        try
        {
            var bodyContent = JsonContent.Create(request, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PostAsync($"{_baseUrl}/text-to-expense", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("text-to-expense endpoint failed.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<TextToExpenseResponseDTO?>(responseContent, JsonOptions.Get());

            return new ApiResponse<TextToExpenseResponseDTO?>(obj, response.StatusCode);
        }
        catch (Exception ex)
        {
            return new ApiResponse<TextToExpenseResponseDTO?>()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Data = null,
            };
        }
    }

    public async Task<ApiResponse<ReceiptLabelsDTO?>> LabelReceiptDetailsAsync(ReceiptDTO receipt)
    {
        try
        {
            var bodyContent = JsonContent.Create(receipt, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PostAsync($"{_baseUrl}/label-receipt-details", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("label-receipt-details endpoint failed.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<ReceiptLabelsDTO?>(responseContent, JsonOptions.Get());

            return new ApiResponse<ReceiptLabelsDTO?>(obj, response.StatusCode);
        }
        catch (Exception ex)
        {
            return new ApiResponse<ReceiptLabelsDTO?>()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Data = null,
            };
        }
    }

    public async Task<ApiResponse<SemanticEmbeddingDTO?>> CreateSemanticEmbeddingAsync(ReceiptLabelsDTO receiptLabels)
    {
        try
        {
            var bodyContent = JsonContent.Create(receiptLabels, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PostAsync($"{_baseUrl}/semantic-embedding", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("semantic-embedding endpoint failed.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<SemanticEmbeddingDTO?>(responseContent, JsonOptions.Get());

            return new ApiResponse<SemanticEmbeddingDTO?>(obj, response.StatusCode);
        }
        catch (Exception ex)
        {
            return new ApiResponse<SemanticEmbeddingDTO?>()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Data = null,
            };
        }
    }

    public async Task<ApiResponse<bool>> UpsertExpenseAsync(ExpenseDTO expense)
    {
        try
        {
            var bodyContent = JsonContent.Create(expense, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PostAsync($"{_baseUrl}/upsert-expense", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("upsert-expense endpoint failed.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<bool>(responseContent, JsonOptions.Get());

            return new ApiResponse<bool>(obj, response.StatusCode);
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Data = false,
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<ExpenseSearchResponseDTO>>> SearchExpensesAsync(ExpenseSearchDTO searchBody)
    {
        try
        {
            var bodyContent = JsonContent.Create(searchBody, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PostAsync($"{_baseUrl}/expense/search", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("expense/search endpoint failed.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<IEnumerable<ExpenseSearchResponseDTO>>(responseContent, JsonOptions.Get());

            return new ApiResponse<IEnumerable<ExpenseSearchResponseDTO>>(obj, response.StatusCode);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<ExpenseSearchResponseDTO>>()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Data = [],
            };
        }
    }
}

