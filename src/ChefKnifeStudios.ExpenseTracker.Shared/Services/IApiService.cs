using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface IApiService
{
    Task<ApiResponse<ReceiptResponse?>> ScanReceiptAsync(Stream fileStream);
}
