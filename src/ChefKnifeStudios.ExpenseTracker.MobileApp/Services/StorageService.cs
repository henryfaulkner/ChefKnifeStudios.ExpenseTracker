using ChefKnifeStudios.ExpenseTracker.Shared;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class StorageService : IStorageService
{
    readonly HttpClient _httpClient;
    readonly ILogger<StorageService> _logger;
    readonly string _baseUrl = string.Empty;

    public StorageService(
        IHttpClientFactory httpClientFactory,
        ILogger<StorageService> logger, 
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("ExpenseTrackerAPI");
        _logger = logger;
        _baseUrl = configuration.GetValue<string>("ApiBaseUrl") ?? string.Empty;
        _baseUrl += "/storage";
    }

    public async Task<ApiResponse<bool>> AddExpenseAsync(ExpenseDTO expenseDTO)
    {
        try
        {
            var bodyContent = JsonContent.Create(expenseDTO, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PostAsync($"{_baseUrl}/expense", bodyContent);
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

    public async Task<ApiResponse<bool>> UpdateExpenseCostAsync(int expenseId, decimal newCost)
    {
        try
        { 
            var bodyContent = JsonContent.Create(newCost, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PatchAsync($"{_baseUrl}/expense/{expenseId}/price", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("update expense cost endpoint failed.");
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

    public async Task<ApiResponse<bool>> DeleteExpenseCostAsync(int expenseId)
    {
        try
        {
            var bodyContent = JsonContent.Create(new object { }, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PatchAsync($"{_baseUrl}/expense/{expenseId}/delete", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("delete expense endpoint failed.");
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

    public async Task<ApiResponse<bool>> AddBudgetAsync(BudgetDTO budgetDTO)
    {
        try
        {
            var bodyContent = JsonContent.Create(budgetDTO, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PostAsync($"{_baseUrl}/budget", bodyContent);
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
            var bodyContent = JsonContent.Create(budgetDTO, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PutAsync($"{_baseUrl}/budget", bodyContent);
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
            var response = await _httpClient.GetAsync($"{_baseUrl}/budgets");
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
            var response = await _httpClient
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

    public async Task<ApiResponse<IEnumerable<RecurringExpenseConfigDTO>>> GetRecurringExpensesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/recurring-expense");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Get recurring expenses endpoint failed.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<IEnumerable<RecurringExpenseConfigDTO>>(responseContent, JsonOptions.Get());

            return new ApiResponse<IEnumerable<RecurringExpenseConfigDTO>>(obj ?? [], response.StatusCode);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<RecurringExpenseConfigDTO>>()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Data = [],
            };
        }
    }

    public async Task<ApiResponse<bool>> AddRecurringExpenseAsync(RecurringExpenseConfigDTO recurringExpense)
    {
        try
        {
            var bodyContent = JsonContent.Create(recurringExpense, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PostAsync($"{_baseUrl}/recurring-expense", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Add recurring expense endpoint failed.");
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

    public async Task<ApiResponse<bool>> DeleteRecurringExpenseAsync(int recurringExpenseId)
    {
        try
        {
            var bodyContent = JsonContent.Create(new object { }, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.PatchAsync($"{_baseUrl}/recurring-expense/{recurringExpenseId}/delete", bodyContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Delete recurring expense endpoint failed.");
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
