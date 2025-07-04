﻿using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.Data.Search;
using ChefKnifeStudios.ExpenseTracker.Data.Specifications;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using Microsoft.SemanticKernel.Connectors.PgVector;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ChefKnifeStudios.ExpenseTracker.BL.Services;

public interface IStorageService
{
    Task<bool> AddExpenseAsync(ExpenseDTO expenseDTO);
    Task<bool> AddBudgetAsync(Budget budget);
    Task<bool> UpdateBudgetAsync(Budget budget);
    Task<IEnumerable<BudgetDTO>> GetBudgetsAsync();
    Task<PagedResult<Budget>> SearchBudgetsAsync(string? searchText, int pageSize, int pageNumber);
    Task<bool> AddRecurringExpenseAsync(RecurringExpenseConfig recurringExpense);
    Task ProcessRecurringExpensesAsync();
}

public class StorageService : IStorageService
{
    private readonly IRepository<Expense> _expenseRepository;
    private readonly IRepository<Budget> _budgetRepository;
    private readonly IRepository<RecurringExpenseConfig> _recurringExpenseRepository;
    private readonly IBudgetSearchRepository _budgetSearchRepository;
    private readonly ISemanticService _semanticService;
    private readonly PostgresVectorStore _vectorStore;

    public StorageService(
        IRepository<Expense> expenseRepository,
        IRepository<Budget> budgetRepository,
        IRepository<RecurringExpenseConfig> recurringExpenseRepository,
        IBudgetSearchRepository budgetSearchRepository,
        ISemanticService semanticService,
        PostgresVectorStore vectorStore)
    {
        _expenseRepository = expenseRepository;
        _budgetRepository = budgetRepository;
        _recurringExpenseRepository = recurringExpenseRepository;
        _budgetSearchRepository = budgetSearchRepository;
        _semanticService = semanticService;
        _vectorStore = vectorStore;
    }

    public async Task<bool> AddExpenseAsync(ExpenseDTO expenseDTO)
    {
        // Determine the month/year for the budget
        var now = DateTime.UtcNow;
        var budgetName = now.ToString("MMMM yyyy");
        var startDate = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)), DateTimeKind.Utc);

        // Try to find an existing budget for this month/year
        var budgets = await _budgetRepository.ListAsync(new GetBudgetsSpec());
        var budget = budgets.FirstOrDefault(b => b.Name == budgetName);

        // If not found, create a new budget for this month
        if (budget == null)
        {
            budget = new Budget
            {
                Name = budgetName,
                StartDateUtc = startDate,
                EndDateUtc = endDate,
                ExpenseBudget = 0 // or a default value
            };
            await _budgetRepository.AddAsync(budget);
        }

        // Assign the budget ID to the expense
        var expense = expenseDTO.MapToModel();
        expense.BudgetId = budget.Id;

        await _expenseRepository.AddAsync(expense);

        if (expense.ExpenseSemantic is not null)
            expense.ExpenseSemantic.ExpenseId = expense.Id;
        return await UpsertExpense(expense);
    }

    public async Task<bool> AddBudgetAsync(Budget budget)
    {
        budget.StartDateUtc = DateTime.SpecifyKind(budget.StartDateUtc, DateTimeKind.Utc);
        budget.EndDateUtc = DateTime.SpecifyKind(budget.EndDateUtc, DateTimeKind.Utc);
        await _budgetRepository.AddAsync(budget);
        return true;
    }

    public async Task<bool> UpdateBudgetAsync(Budget budget)
    {
        budget.StartDateUtc = DateTime.SpecifyKind(budget.StartDateUtc, DateTimeKind.Utc);
        budget.EndDateUtc = DateTime.SpecifyKind(budget.EndDateUtc, DateTimeKind.Utc);
        await _budgetRepository.UpdateAsync(budget);
        return true;
    }

    public async Task<IEnumerable<BudgetDTO>> GetBudgetsAsync()
    {
        var budgets = await _budgetRepository.ListAsync(new GetBudgetsSpec());
        return budgets.Select(x => x.MapToDTO());
    }

    public async Task<PagedResult<Budget>> SearchBudgetsAsync(string? searchText, int pageSize, int pageNumber)
    {
        return await _budgetSearchRepository.GetFilteredResultAsync(searchText, pageSize, pageNumber);
    }

    public async Task<bool> AddRecurringExpenseAsync(RecurringExpenseConfig recurringExpense)
    {
        await _recurringExpenseRepository.AddAsync(recurringExpense);
        return true;
    }

    public async Task ProcessRecurringExpensesAsync()
    {
        var recurringExpenses = await _recurringExpenseRepository.ListAsync();

        List<Expense> newExpenses = [];
        List<Budget> newBudgets = [];
        foreach (var recurringExpense in recurringExpenses)
        {
            var labels = JsonSerializer.Deserialize<IEnumerable<string>>(recurringExpense.LabelsJson, Shared.JsonOptions.Get()) ?? [];
            SemanticEmbeddingDTO embedding = await _semanticService.CreateSemanticEmbeddingAsync(
                new ReceiptLabelsDTO() { 
                    Name = recurringExpense.Name, 
                    Labels = labels,
                }
            );
            var expense = new ExpenseDTO()
            {
                Name = recurringExpense.Name,
                Labels = labels,
                Cost = recurringExpense.Cost,
                IsRecurring = true,
                ExpenseSemantic = new()
                {
                    Labels = JsonSerializer.Serialize(labels, Shared.JsonOptions.Get()),
                    SemanticEmbedding = embedding.Embedding,
                },
            };
            await AddExpenseAsync(expense);
        }
    }

    private async Task<bool> UpsertExpense(Expense expense)
    {
        try
        {
            // Get and create collection if it doesn't exist.
            var collectionName = "ExpenseSemantics";
            var expenseSemanticCollection = _vectorStore.GetCollection<int, ExpenseSemantic>(collectionName);
            await expenseSemanticCollection.EnsureCollectionExistsAsync().ConfigureAwait(false);

            await expenseSemanticCollection.UpsertAsync(expense.ExpenseSemantic);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }
}