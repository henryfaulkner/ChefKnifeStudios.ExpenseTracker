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

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class ApiService : IApiService
{
    readonly string _baseUrl = string.Empty;

    public ApiService(IConfiguration configuration)
    {
        _baseUrl = configuration.GetValue<string>("ApiBaseUrl") ?? string.Empty;
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
            var obj = JsonSerializer.Deserialize<IEnumerable<ReceiptDTO>?>(content);

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
            var obj = JsonSerializer.Deserialize<ReceiptLabelsDTO?>(responseContent);

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

    public async Task<ApiResponse<SemanticEmbeddingDTO?>> CreateSemanticEmbedding(ReceiptLabelsDTO receiptLabels)
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
            var obj = JsonSerializer.Deserialize<SemanticEmbeddingDTO?>(responseContent);

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
}

