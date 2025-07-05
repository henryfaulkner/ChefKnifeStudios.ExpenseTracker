using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using ChefKnifeStudios.ExpenseTracker.Shared;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class StorageService : IStorageService
{
    readonly ILogger<StorageService> _logger;
    readonly string _baseUrl = string.Empty;

    public StorageService(ILogger<StorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _baseUrl = configuration.GetValue<string>("ApiBaseUrl") ?? string.Empty;
        _baseUrl += "/storage";
    }

    public async Task<ApiResponse<bool>> AddExpenseAsync(ExpenseDTO expenseDTO)
    {
        try
        {
            using var httpClient = new HttpClient();
            var bodyContent = JsonContent.Create(expenseDTO, new MediaTypeHeaderValue("application/json"));

            var response = await httpClient.PostAsync($"{_baseUrl}/expense", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("expense endpoint failed.");
            }

            return new ApiResponse<bool>(true, response.StatusCode);
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

    public async Task<ApiResponse<bool>> AddBudgetAsync(BudgetDTO budgetDTO)
    {
        try
        {
            using var httpClient = new HttpClient();
            var bodyContent = JsonContent.Create(budgetDTO, new MediaTypeHeaderValue("application/json"));

            var response = await httpClient.PostAsync($"{_baseUrl}/budget", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("budget endpoint failed.");
            }

            return new ApiResponse<bool>(true, response.StatusCode);
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

    public async Task<ApiResponse<bool>> UpdateBudgetAsync(BudgetDTO budgetDTO)
    {
        try
        {
            using var httpClient = new HttpClient();
            var bodyContent = JsonContent.Create(budgetDTO, new MediaTypeHeaderValue("application/json"));

            var response = await httpClient.PutAsync($"{_baseUrl}/budget", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("update budget endpoint failed.");
            }

            return new ApiResponse<bool>(true, response.StatusCode);
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

    public async Task<ApiResponse<IEnumerable<BudgetDTO>?>> GetBudgetsAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            
            var response = await httpClient.GetAsync($"{_baseUrl}/budgets");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("budgets endpoint failed.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<IEnumerable<BudgetDTO>?>(responseContent, JsonOptions.Get());

            return new ApiResponse<IEnumerable<BudgetDTO>?>(obj, response.StatusCode);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<BudgetDTO>?>()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Data = null,
            };
        }
    }

    public async Task<ApiResponse<PagedResultDTO<BudgetDTO>?>> SearchBudgetsAsync(
        string? searchText,
        int pageSize,
        int pageNumber)
    {
        try
        {
            using var httpClient = new HttpClient();
            
            var response = await httpClient
                .GetAsync($"{_baseUrl}/budgets/search?searchText={searchText}&pageSize={pageSize}&pageNumber={pageNumber}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(" endpoint failed.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<PagedResultDTO<BudgetDTO>?>(responseContent, JsonOptions.Get());

            return new ApiResponse<PagedResultDTO<BudgetDTO>?>(obj, response.StatusCode);
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResultDTO<BudgetDTO>?>()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Data = null,
            };
        }
    }

    public async Task<ApiResponse<bool>> AddRecurringExpenseAsync(RecurringExpenseConfigDTO recurringExpense)
    {
        try
        {
            using var httpClient = new HttpClient();
            var bodyContent = JsonContent.Create(recurringExpense, new MediaTypeHeaderValue("application/json"));

            var response = await httpClient.PostAsync($"{_baseUrl}/recurring-expense", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Recurring expense endpoint failed.");
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
}
