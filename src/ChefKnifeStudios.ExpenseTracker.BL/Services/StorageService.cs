using ChefKnifeStudios.ExpenseTracker.Data;
using ChefKnifeStudios.ExpenseTracker.Data.Constants;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.Data.Search;
using ChefKnifeStudios.ExpenseTracker.Data.Specifications;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.PgVector;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ChefKnifeStudios.ExpenseTracker.BL.Services;

public interface IStorageService
{
    Task<bool> AddExpenseAsync(ExpenseDTO expenseDTO, Guid appId, CancellationToken cancellationToken = default);
    Task<bool> UpdateExpenseCostAsync(int expenseId, decimal newCost, Guid appId, CancellationToken cancellationToken = default);
    Task<bool> DeleteExpenseCostAsync(int expenseId, Guid appId, CancellationToken cancellationToken = default);
    Task<bool> AddBudgetAsync(Budget budget, Guid appId, CancellationToken cancellationToken = default);
    Task<bool> UpdateBudgetAsync(Budget budget, Guid appId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BudgetDTO>> GetBudgetsAsync(Guid appId, CancellationToken cancellationToken = default);
    Task<PagedResult<Budget>> SearchBudgetsAsync(string? searchText, int pageSize, int pageNumber, Guid appId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RecurringExpenseConfigDTO>> GetRecurringExpensesAsync(Guid appId, CancellationToken cancellationToken = default);
    Task<bool> AddRecurringExpenseAsync(RecurringExpenseConfig recurringExpense, Guid appId, CancellationToken cancellationToken = default);
    Task<bool> DeleteRecurringExpenseAsync(int recurringExpenseConfigId, Guid appId, CancellationToken cancellationToken = default);
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
    private readonly AppDbContext _dbContext;
    private readonly ILogger<StorageService> _logger;

    public StorageService(
        IRepository<Expense> expenseRepository,
        IRepository<Budget> budgetRepository,
        IRepository<RecurringExpenseConfig> recurringExpenseRepository,
        IBudgetSearchRepository budgetSearchRepository,
        ISemanticService semanticService,
        PostgresVectorStore vectorStore,
        AppDbContext dbContext,
        ILogger<StorageService> logger)
    {
        _expenseRepository = expenseRepository;
        _budgetRepository = budgetRepository;
        _recurringExpenseRepository = recurringExpenseRepository;
        _budgetSearchRepository = budgetSearchRepository;
        _semanticService = semanticService;
        _vectorStore = vectorStore;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> AddExpenseAsync(ExpenseDTO expenseDTO, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting AddExpenseAsync for AppId: {AppId}, Expense Name: {ExpenseName}", appId, expenseDTO.Name);
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var budgetName = now.ToString("MMMM yyyy");
            var startDate = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)), DateTimeKind.Utc);

            var budgets = await _budgetRepository.ListAsync(new GetBudgetsSpec(appId), cancellationToken);
            var budget = budgets.FirstOrDefault(b => b.Name == budgetName);

            if (budget == null)
            {
                budget = new Budget
                {
                    Name = budgetName,
                    StartDateUtc = startDate,
                    EndDateUtc = endDate,
                    ExpenseBudget = 0,
                    AppId = appId,
                };
                await _budgetRepository.AddAsync(budget, cancellationToken);
                _logger.LogInformation("Created new budget for AppId: {AppId}, Budget Name: {BudgetName}", appId, budgetName);
            }

            if (!expenseDTO.Labels.Any())
            {
                string textPrompt = $"Name: {expenseDTO.Name} - Cost: {expenseDTO.Cost}";
                TextToExpenseResponseDTO tteRes = await _semanticService.TextToExpenseAsync(textPrompt, appId, cancellationToken);
                expenseDTO.Labels = tteRes.Labels ?? [];
            }

            var expense = expenseDTO.MapToModel();
            expense.BudgetId = budget.Id;
            expense.AppId = appId;
            if (expense.ExpenseSemantic is not null) expense.ExpenseSemantic.AppId = appId;

            await _expenseRepository.AddAsync(expense, cancellationToken);
            _logger.LogInformation("Added expense for AppId: {AppId}, ExpenseId: {ExpenseId}, Name: {ExpenseName}", appId, expense.Id, expense.Name);

            if (expense.ExpenseSemantic is not null) expense.ExpenseSemantic.ExpenseId = expense.Id;

            var upsertResult = await UpsertExpense(expense, cancellationToken);
            if (!upsertResult)
            {
                _logger.LogWarning("UpsertExpense failed for ExpenseId: {ExpenseId}, AppId: {AppId}", expense.Id, appId);
            }

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Transaction committed for AddExpenseAsync, AppId: {AppId}", appId);
            return upsertResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Exception in AddExpenseAsync for AppId: {AppId}, Expense Name: {ExpenseName}", appId, expenseDTO.Name);
            return false;
        }
    }

    public async Task<bool> UpdateExpenseCostAsync(int expenseId, decimal newCost, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting UpdateExpenseCostAsync for AppId: {AppId}, Expense Id: {expenseId}, New Cost: {newCost}", appId, expenseId, newCost);
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var expense = (await _expenseRepository.ListAsync(new GetExpenseByIdSpec(expenseId, appId), cancellationToken))
                .FirstOrDefault();
            if (expense is null)
            {
                _logger.LogWarning("Expense {expenseId} could not be found. AppId: {AppId}, New Cost: {newCost}", expenseId, appId, newCost);
                return false;
            }

            expense.Cost = newCost;
            expense.ModifiedOnUtc = DateTime.UtcNow;
            int updatedRecords = await _expenseRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated expense for AppId: {AppId}, Expense Id: {expenseId}, New Cost: {newCost}", appId, expenseId, newCost);

            if (expense.ExpenseSemantic is not null) expense.ExpenseSemantic.ExpenseId = expense.Id;

            var upsertResult = await UpsertExpense(expense, cancellationToken);
            if (!upsertResult)
            {
                _logger.LogWarning("UpsertExpense failed for ExpenseId: AppId: {AppId}, Expense Id: {expenseId}, New Cost: {newCost}", appId, expenseId, newCost);
            }

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Transaction committed for AddExpenseAsync: {AppId}, Expense Id: {expenseId}, New Cost: {newCost}", appId, expenseId, newCost);
            return upsertResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Exception in AddExpenseAsync for AppId: {AppId}, Expense Id: {expenseId}, New Cost: {newCost}", appId, expenseId, newCost);
            return false;
        }
    }

    public async Task<bool> DeleteExpenseCostAsync(int expenseId, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting DeleteExpenseCostAsync for AppId: {AppId}, Expense Id: {expenseId}", appId, expenseId);
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var expense = await _expenseRepository.GetByIdAsync(expenseId, cancellationToken);
            if (expense is null)
            {
                _logger.LogWarning("Expense {expenseId} could not be found. AppId: {AppId}", expenseId, appId);
                return false;
            }

            expense.IsDeleted = true;
            expense.ModifiedOnUtc = DateTime.UtcNow;
            await _expenseRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Marked expense as deleted for AppId: {AppId}, Expense Id: {expenseId}", appId, expenseId);

            if (expense.ExpenseSemantic is not null) expense.ExpenseSemantic.ExpenseId = expense.Id;

            var upsertResult = await DeleteExpense(expense, cancellationToken); // Hard delete expense semantic from vector store
            if (!upsertResult)
            {
                _logger.LogWarning("UpsertExpense failed for ExpenseId: AppId: {AppId}, Expense Id: {expenseId}", appId, expenseId);
            }

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Transaction committed for DeleteExpenseCostAsync: {AppId}, Expense Id: {expenseId}", appId, expenseId);
            return upsertResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Exception in DeleteExpenseCostAsync for AppId: {AppId}, Expense Id: {expenseId}", appId, expenseId);
            return false;
        }
    }

    public async Task<bool> AddBudgetAsync(Budget budget, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting AddBudgetAsync for AppId: {AppId}, Budget Name: {BudgetName}", appId, budget.Name);
        try
        {
            budget.AppId = appId;
            budget.StartDateUtc = DateTime.SpecifyKind(budget.StartDateUtc, DateTimeKind.Utc);
            budget.EndDateUtc = DateTime.SpecifyKind(budget.EndDateUtc, DateTimeKind.Utc);
            await _budgetRepository.AddAsync(budget, cancellationToken);
            _logger.LogInformation("Added budget for AppId: {AppId}, BudgetId: {BudgetId}, Name: {BudgetName}", appId, budget.Id, budget.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AddBudgetAsync for AppId: {AppId}, Budget Name: {BudgetName}", appId, budget.Name);
            return false;
        }
    }

    public async Task<bool> UpdateBudgetAsync(Budget budget, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting UpdateBudgetAsync for AppId: {AppId}, BudgetId: {BudgetId}", appId, budget.Id);
        try
        {
            budget.StartDateUtc = DateTime.SpecifyKind(budget.StartDateUtc, DateTimeKind.Utc);
            budget.EndDateUtc = DateTime.SpecifyKind(budget.EndDateUtc, DateTimeKind.Utc);
            await _budgetRepository.UpdateAsync(budget, cancellationToken);
            _logger.LogInformation("Updated budget for AppId: {AppId}, BudgetId: {BudgetId}", appId, budget.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UpdateBudgetAsync for AppId: {AppId}, BudgetId: {BudgetId}", appId, budget.Id);
            return false;
        }
    }

    public async Task<IEnumerable<BudgetDTO>> GetBudgetsAsync(Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting GetBudgetsAsync for AppId: {AppId}", appId);
        try
        {
            var budgets = await _budgetRepository.ListAsync(new GetBudgetsSpec(appId), cancellationToken);
            _logger.LogInformation("Retrieved {Count} budgets for AppId: {AppId}", budgets.Count(), appId);
            return budgets.Select(x => x.MapToDTO());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetBudgetsAsync for AppId: {AppId}", appId);
            return Enumerable.Empty<BudgetDTO>();
        }
    }

    public async Task<PagedResult<Budget>> SearchBudgetsAsync(string? searchText, int pageSize, int pageNumber, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting SearchBudgetsAsync for AppId: {AppId}, SearchText: {SearchText}", appId, searchText);
        try
        {
            var result = await _budgetSearchRepository.GetFilteredResultAsync(searchText, pageSize, pageNumber, appId, cancellationToken: cancellationToken);
            _logger.LogInformation("SearchBudgetsAsync returned {Count} results for AppId: {AppId}", result.TotalRecords, appId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in SearchBudgetsAsync for AppId: {AppId}, SearchText: {SearchText}", appId, searchText);
            return new PagedResult<Budget>();
        }
    }

    public async Task<IEnumerable<RecurringExpenseConfigDTO>> GetRecurringExpensesAsync(Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting GetRecurringExpensesAsync for AppId: {AppId}", appId);
        try
        {
            var recurringExpenses = await _recurringExpenseRepository.ListAsync(new GetRecurringExpensesSpec(appId), cancellationToken);
            _logger.LogInformation("Getting recurring expenses for AppId: {AppId}", appId);
            return recurringExpenses?.Select(x => x.MapToDTO()) ?? Enumerable.Empty<RecurringExpenseConfigDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetRecurringExpensesAsync for AppId: {AppId}", appId);
            return Enumerable.Empty<RecurringExpenseConfigDTO>();
        }
    }

    public async Task<bool> AddRecurringExpenseAsync(RecurringExpenseConfig recurringExpense, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting AddRecurringExpenseAsync for AppId: {AppId}, Name: {Name}", appId, recurringExpense.Name);
        try
        {
            recurringExpense.AppId = appId;
            await _recurringExpenseRepository.AddAsync(recurringExpense, cancellationToken);
            _logger.LogInformation("Added recurring expense for AppId: {AppId}, RecurringExpenseId: {RecurringExpenseId}, Name: {Name}", appId, recurringExpense.Id, recurringExpense.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AddRecurringExpenseAsync for AppId: {AppId}, Name: {Name}", appId, recurringExpense.Name);
            return false;
        }
    }

    public async Task<bool> DeleteRecurringExpenseAsync(int recurringExpenseConfigId, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting DeleteRecurringExpenseAsync for AppId: {AppId}, Recurring Expense Id: {recurringExpenseConfigId}", appId, recurringExpenseConfigId);
        try
        {
            var recurringExpenseConfig = await _recurringExpenseRepository.GetByIdAsync(recurringExpenseConfigId, cancellationToken);
            if (recurringExpenseConfig is null)
            {
                _logger.LogWarning("Recurring Expense Config {recurringExpenseConfigId} could not be found. AppId: {AppId}", recurringExpenseConfigId, appId);
                return false;
            }

            recurringExpenseConfig.IsDeleted = true;
            recurringExpenseConfig.ModifiedOnUtc = DateTime.UtcNow;
            await _recurringExpenseRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added recurring expense for AppId: {AppId}, Recurring Expense Id: {recurringExpenseConfigId}", appId, recurringExpenseConfigId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in DeleteRecurringExpenseAsync for AppId: {AppId}, Recurring Expense Id: {recurringExpenseConfigId}", appId, recurringExpenseConfigId);
            return false;
        }
    }

    public async Task ProcessRecurringExpensesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting ProcessRecurringExpensesAsync");
        try
        {
            var recurringExpenses = await _recurringExpenseRepository.ListAsync(cancellationToken);

            foreach (var recurringExpense in recurringExpenses)
            {
                var labels = JsonSerializer.Deserialize<IEnumerable<string>>(recurringExpense.LabelsJson, Shared.JsonOptions.Get()) ?? [];
                SemanticEmbeddingDTO embedding = await _semanticService.CreateSemanticEmbeddingAsync(
                    new ReceiptLabelsDTO()
                    {
                        Name = recurringExpense.Name,
                        CreatedOn = recurringExpense.CreatedOnUtc,
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
                var result = await AddExpenseAsync(expense, recurringExpense.AppId, cancellationToken);
                if (!result)
                {
                    _logger.LogWarning("Failed to add expense for recurring expense. RecurringExpenseId: {RecurringExpenseId}, AppId: {AppId}", recurringExpense.Id, recurringExpense.AppId);
                }
            }
            
            _logger.LogInformation("ProcessRecurringExpensesAsync processed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in ProcessRecurringExpensesAsync");
        }
    }

    private async Task<bool> UpsertExpense(Expense expense, CancellationToken cancellationToken = default)
    {
        try
        {
            if (expense.ExpenseSemantic == null) throw new ArgumentNullException(nameof(expense.ExpenseSemantic));

            var collectionName = Collections.ExpenseSemantics;
            var expenseSemanticCollection = _vectorStore.GetCollection<int, ExpenseSemantic>(collectionName);
            await expenseSemanticCollection.EnsureCollectionExistsAsync().ConfigureAwait(false);
            await expenseSemanticCollection.UpsertAsync(expense.ExpenseSemantic, cancellationToken);
            _logger.LogInformation("Upserted expense semantic for ExpenseId: {ExpenseId}", expense.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UpsertExpense for ExpenseId: {ExpenseId}", expense.Id);
            return false;
        }
        return true;
    }

    private async Task<bool> DeleteExpense(Expense expense, CancellationToken cancellationToken = default)
    {
        try
        {
            if (expense.ExpenseSemantic == null) throw new ArgumentNullException(nameof(expense.ExpenseSemantic));

            var collectionName = Collections.ExpenseSemantics;
            var expenseSemanticCollection = _vectorStore.GetCollection<int, ExpenseSemantic>(collectionName);
            await expenseSemanticCollection.EnsureCollectionExistsAsync().ConfigureAwait(false);
            await expenseSemanticCollection.DeleteAsync(expense.Id, cancellationToken);
            _logger.LogInformation("Deleted expense semantic for ExpenseId: {ExpenseId}", expense.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in DeleteExpense for ExpenseId: {ExpenseId}", expense.Id);
            return false;
        }
        return true;
    }
}