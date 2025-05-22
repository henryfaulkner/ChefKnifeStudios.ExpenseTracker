using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class StorageService : IStorageService
{
    readonly IRepository<Expense> _expenseRepository;
    readonly IRepository<Budget> _budgetRepository;

    public StorageService(IRepository<Expense> expenseRepository, IRepository<Budget> budgetRepository)
    {
        _expenseRepository = expenseRepository;
        _budgetRepository = budgetRepository;
    }

    public async Task AddExpense(ExpenseDTO expenseDTO)
    {
        Expense expense = expenseDTO.MapToModel();
        await _expenseRepository.AddAsync(expense);
    }

    public async Task AddBudget(BudgetDTO budgetDTO)
    {
        Budget budget = budgetDTO.MapToModel();
        await _budgetRepository.AddAsync(budget);
    }

    public async Task<BudgetWithExpensesDTO> GetBudgetWithExpenses()
    {
        
    }
}
