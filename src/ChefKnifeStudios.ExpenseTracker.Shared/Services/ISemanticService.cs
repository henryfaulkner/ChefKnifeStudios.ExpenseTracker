using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface ISemanticService
{
    Task<ApiResponse<IEnumerable<ReceiptDTO>?>> ScanReceiptAsync(Stream fileStream);
    Task<ApiResponse<ReceiptLabelsDTO?>> LabelReceiptDetailsAsync(ReceiptDTO receipt);
    Task<ApiResponse<SemanticEmbeddingDTO?>> CreateSemanticEmbeddingAsync(ReceiptLabelsDTO receiptLabels);
    Task<ApiResponse<bool>> UpsertExpenseAsync(ExpenseDTO expense);
    Task<ApiResponse<IEnumerable<ExpenseDTO>>> SearchExpensesAsync(ExpenseSearchDTO searchBody);
}
