using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface IStorageService
{
    Task AddBudgetAsync(BudgetDTO budgetDTO);
    Task AddExpenseAsync(ExpenseDTO expenseDTO);
    Task<PagedResultDTO<BudgetDTO>> SearchBudgetsAsync(
        string? searchText,
        int pageSize,
        int pageNumber
    );
}
