using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Data.Search;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class StorageService : IStorageService
{
    readonly IRepository<Expense> _expenseRepository;
    readonly IRepository<Budget> _budgetRepository;
    readonly IBudgetSearchRepository _budgetSearchRepository;

    public StorageService(IRepository<Expense> expenseRepository, 
        IRepository<Budget> budgetRepository,
        IBudgetSearchRepository budgetSearchRepository)
    {
        _expenseRepository = expenseRepository;
        _budgetRepository = budgetRepository;
        _budgetSearchRepository = budgetSearchRepository;
    }

    public async Task AddExpenseAsync(ExpenseDTO expenseDTO)
    {
        Expense expense = expenseDTO.MapToModel();
        await _expenseRepository.AddAsync(expense);
    }

    public async Task AddBudgetAsync(BudgetDTO budgetDTO)
    {
        Budget budget = budgetDTO.MapToModel();
        await _budgetRepository.AddAsync(budget);
    }

    public async Task<PagedResultDTO<BudgetDTO>> SearchBudgetsAsync(
        string? searchText,
        int pageSize,
        int pageNumber)
    {
        PagedResult<Budget> pagedResult = await _budgetSearchRepository.GetFilteredResultAsync(searchText, pageSize, pageNumber);
        return pagedResult.MapToDTO();
    }
}
