using ChefKnifeStudios.ExpenseTracker.Data.Models;
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
    Task<bool> AddExpenseAsync(ExpenseDTO expenseDTO, Guid appId, CancellationToken cancellationToken = default);
    Task<bool> AddBudgetAsync(Budget budget, Guid appId, CancellationToken cancellationToken = default);
    Task<bool> UpdateBudgetAsync(Budget budget, Guid appId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BudgetDTO>> GetBudgetsAsync(Guid appId, CancellationToken cancellationToken = default);
    Task<PagedResult<Budget>> SearchBudgetsAsync(string? searchText, int pageSize, int pageNumber, Guid appId, CancellationToken cancellationToken = default);
    Task<bool> AddRecurringExpenseAsync(RecurringExpenseConfig recurringExpense, Guid appId, CancellationToken cancellationToken = default);
    Task ProcessRecurringExpensesAsync(CancellationToken cancellationToken = default);
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

    public async Task<bool> AddExpenseAsync(ExpenseDTO expenseDTO, Guid appId, CancellationToken cancellationToken = default)
    {
        // Determine the month/year for the budget
        var now = DateTime.UtcNow;
        var budgetName = now.ToString("MMMM yyyy");
        var startDate = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)), DateTimeKind.Utc);

        // Try to find an existing budget for this month/year
        var budgets = await _budgetRepository.ListAsync(new GetBudgetsSpec(appId));
        var budget = budgets.FirstOrDefault(b => b.Name == budgetName);

        // If not found, create a new budget for this month
        if (budget == null)
        {
            budget = new Budget
            {
                Name = budgetName,
                StartDateUtc = startDate,
                EndDateUtc = endDate,
                ExpenseBudget = 0, // or a default value
                AppId = appId,
            };
            await _budgetRepository.AddAsync(budget, cancellationToken);
        }

        // Assign the budget ID to the expense
        var expense = expenseDTO.MapToModel();
        expense.BudgetId = budget.Id;
        expense.AppId = appId;

        await _expenseRepository.AddAsync(expense, cancellationToken);

        if (expense.ExpenseSemantic is not null)
            expense.ExpenseSemantic.ExpenseId = expense.Id;
        return await UpsertExpense(expense, cancellationToken);
    }

    public async Task<bool> AddBudgetAsync(Budget budget, Guid appId, CancellationToken cancellationToken = default)
    {
        budget.AppId = appId;
        budget.StartDateUtc = DateTime.SpecifyKind(budget.StartDateUtc, DateTimeKind.Utc);
        budget.EndDateUtc = DateTime.SpecifyKind(budget.EndDateUtc, DateTimeKind.Utc);
        await _budgetRepository.AddAsync(budget, cancellationToken);
        return true;
    }

    public async Task<bool> UpdateBudgetAsync(Budget budget, Guid appId, CancellationToken cancellationToken = default)
    {
        budget.StartDateUtc = DateTime.SpecifyKind(budget.StartDateUtc, DateTimeKind.Utc);
        budget.EndDateUtc = DateTime.SpecifyKind(budget.EndDateUtc, DateTimeKind.Utc);
        await _budgetRepository.UpdateAsync(budget, cancellationToken);
        return true;
    }

    public async Task<IEnumerable<BudgetDTO>> GetBudgetsAsync(Guid appId, CancellationToken cancellationToken = default)
    {
        var budgets = await _budgetRepository.ListAsync(new GetBudgetsSpec(appId), cancellationToken);
        return budgets.Select(x => x.MapToDTO());
    }

    public async Task<PagedResult<Budget>> SearchBudgetsAsync(string? searchText, int pageSize, int pageNumber, Guid appId, CancellationToken cancellationToken = default)
    {
        return await _budgetSearchRepository.GetFilteredResultAsync(searchText, pageSize, pageNumber, appId, cancellationToken: cancellationToken);
    }

    public async Task<bool> AddRecurringExpenseAsync(RecurringExpenseConfig recurringExpense, Guid appId, CancellationToken cancellationToken = default)
    {
        recurringExpense.AppId = appId;
        await _recurringExpenseRepository.AddAsync(recurringExpense, cancellationToken);
        return true;
    }

    public async Task ProcessRecurringExpensesAsync(CancellationToken cancellationToken = default)
    {
        var recurringExpenses = await _recurringExpenseRepository.ListAsync(cancellationToken);

        List<Expense> newExpenses = [];
        List<Budget> newBudgets = [];
        foreach (var recurringExpense in recurringExpenses)
        {
            var labels = JsonSerializer.Deserialize<IEnumerable<string>>(recurringExpense.LabelsJson, Shared.JsonOptions.Get()) ?? [];
            SemanticEmbeddingDTO embedding = await _semanticService.CreateSemanticEmbeddingAsync(
                new ReceiptLabelsDTO() { 
                    Name = recurringExpense.Name, 
                    Labels = labels,
                },
                recurringExpense.AppId,
                cancellationToken
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
            await AddExpenseAsync(expense, recurringExpense.AppId, cancellationToken);
        }
    }

    private async Task<bool> UpsertExpense(Expense expense, CancellationToken cancellationToken = default)
    {
        try
        {
            if (expense.ExpenseSemantic == null) throw new ArgumentNullException(nameof(expense.ExpenseSemantic));

            // Get and create collection if it doesn't exist.
            var collectionName = "ExpenseSemantics";
            var expenseSemanticCollection = _vectorStore.GetCollection<int, ExpenseSemantic>(collectionName);
            await expenseSemanticCollection.EnsureCollectionExistsAsync().ConfigureAwait(false);
            await expenseSemanticCollection.UpsertAsync(expense.ExpenseSemantic, cancellationToken);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }
}