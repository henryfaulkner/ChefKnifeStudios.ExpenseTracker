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

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class ApiService : IApiService
{
    readonly string _baseUrl = string.Empty;

    public ApiService(IConfiguration configuration)
    {
        _baseUrl = configuration.GetValue<string>("ApiBaseUrl") ?? string.Empty;
    }

    public async Task<ApiResponse<ReceiptResponse?>> ScanReceiptAsync(Stream fileStream)
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
                // Handle the error response directly.
                throw new HttpRequestException("scan-receipt endpoint failed.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<ReceiptResponse?>(content);

            return new ApiResponse<ReceiptResponse?>(obj);
        }
        catch (Exception ex)
        {
            return new ApiResponse<ReceiptResponse?>()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Data = null,
            };
        }
    }

}

