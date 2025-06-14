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

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class SemanticService : ISemanticService
{
    readonly string _baseUrl = string.Empty;

    public SemanticService(IConfiguration configuration)
    {
        _baseUrl = configuration.GetValue<string>("ApiBaseUrl") ?? string.Empty;
        _baseUrl += "/semantic";
    }

    public async Task<ApiResponse<IEnumerable<ReceiptDTO>?>> ScanReceiptAsync(Stream fileStream)
    {
        try
        { 
            using var httpClient = new HttpClient();
            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(fileContent, "file", "receipt.jpg");

            var response = await httpClient.PostAsync($"{_baseUrl}/scan-receipt", form);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("scan-receipt endpoint failed.");
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

    public async Task<ApiResponse<ReceiptLabelsDTO?>> LabelReceiptDetailsAsync(ReceiptDTO receipt)
    {
        try
        {
            using var httpClient = new HttpClient();
            var bodyContent = JsonContent.Create(receipt, new MediaTypeHeaderValue("application/json"));

            var response = await httpClient.PostAsync($"{_baseUrl}/label-receipt-details", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("label-receipt-details endpoint failed.");
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
            using var httpClient = new HttpClient();
            var bodyContent = JsonContent.Create(receiptLabels, new MediaTypeHeaderValue("application/json"));

            var response = await httpClient.PostAsync($"{_baseUrl}/semantic-embedding", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("semantic-embedding endpoint failed.");
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
            using var httpClient = new HttpClient();
            var bodyContent = JsonContent.Create(expense, new MediaTypeHeaderValue("application/json"));

            var response = await httpClient.PostAsync($"{_baseUrl}/upsert-expense", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("upsert-expense endpoint failed.");
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

    public async Task<ApiResponse<IEnumerable<ExpenseDTO>>> SearchExpensesAsync(ExpenseSearchDTO searchBody)
    {
        try
        {
            using var httpClient = new HttpClient();
            var bodyContent = JsonContent.Create(searchBody, new MediaTypeHeaderValue("application/json"));

            var response = await httpClient.PostAsync($"{_baseUrl}/expense/search", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("expense/search endpoint failed.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<IEnumerable<ExpenseDTO>>(responseContent, JsonOptions.Get());

            return new ApiResponse<IEnumerable<ExpenseDTO>>(obj, response.StatusCode);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<ExpenseDTO>>()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Data = [],
            };
        }
    }
}

