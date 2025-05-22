using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface IStorageService
{
    Task AddBudget(BudgetDTO budgetDTO);
    Task AddExpense(ExpenseDTO expenseDTO);
    Task<BudgetWithExpensesDTO> GetBudgetWithExpenses();
}
