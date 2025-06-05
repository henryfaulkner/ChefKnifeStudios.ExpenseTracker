using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface IApiService
{
    Task<ApiResponse<IEnumerable<ReceiptDTO>?>> ScanReceiptAsync(Stream fileStream);
    Task<ApiResponse<ReceiptLabelsDTO?>> LabelReceiptDetailsAsync(ReceiptDTO receipt);
}
