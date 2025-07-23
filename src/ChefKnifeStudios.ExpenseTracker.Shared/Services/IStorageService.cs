using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface IStorageService
{
    Task<ApiResponse<bool>> AddBudgetAsync(BudgetDTO budgetDTO);
    Task<ApiResponse<bool>> UpdateBudgetAsync(BudgetDTO budgetDTO);
    Task<ApiResponse<bool>> AddExpenseAsync(ExpenseDTO expenseDTO);
    Task<ApiResponse<IEnumerable<BudgetDTO>?>> GetBudgetsAsync();
    Task<ApiResponse<PagedResultDTO<BudgetDTO>?>> SearchBudgetsAsync(
        string? searchText,
        int pageSize,
        int pageNumber
    );
    Task<ApiResponse<IEnumerable<RecurringExpenseConfigDTO>>> GetRecurringExpensesAsync();
    Task<ApiResponse<bool>> AddRecurringExpenseAsync(RecurringExpenseConfigDTO recurringExpense);
    Task<ApiResponse<bool>> DeleteRecurringExpenseAsync(int recurringExpenseId);
}
