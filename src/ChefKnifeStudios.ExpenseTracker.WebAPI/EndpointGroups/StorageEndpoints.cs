using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Data.Search;
using ChefKnifeStudios.ExpenseTracker.Data.Specifications;
using Microsoft.AspNetCore.Mvc;
using Azure.Core;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace ChefKnifeStudios.ExpenseTracker.WebAPI.EndpointGroups;

public static class StorageEndpoints
{
    public static IEndpointRouteBuilder MapStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/storage")
            .WithName("Storage");

        // Add Expense
        group.MapPost("/expense", async (
            [FromBody] ExpenseDTO expenseDTO,
            [FromServices] IRepository<Expense> expenseRepository,
            [FromServices] IRepository<Budget> budgetRepository,
            SqliteVectorStore vectorStore) =>
        {
            // Determine the month/year for the budget
            var now = DateTime.Now;
            var budgetName = now.ToString("MMMM yyyy");
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            // Try to find an existing budget for this month/year
            var budgets = await budgetRepository.ListAsync(new GetBudgetsSpec());
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
                await budgetRepository.AddAsync(budget);
            }

            // Assign the budget ID to the expense
            var expense = expenseDTO.MapToModel();
            expense.BudgetId = budget.Id;

            await expenseRepository.AddAsync(expense);
            await UpsertExpense(expense, vectorStore);
            return Results.Ok();
        })
        .WithName("AddExpense")
        .Accepts<ExpenseDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        // Add Budget
        group.MapPost("/budget", async (
            [FromBody] BudgetDTO budgetDTO,
            [FromServices] IRepository<Budget> budgetRepository) =>
        {
            var budget = budgetDTO.MapToModel();
            await budgetRepository.AddAsync(budget);
            return Results.Ok();
        })
        .WithName("AddBudget")
        .Accepts<BudgetDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        // Update Budget
        group.MapPut("/budget", async (
            [FromBody] BudgetDTO budgetDTO,
            [FromServices] IRepository<Budget> budgetRepository) =>
        {
            // Find the existing budget by Id
            var existing = await budgetRepository.GetByIdAsync(budgetDTO.Id);
            if (existing == null)
                return Results.NotFound();

            // Update properties
            existing.Name = budgetDTO.Name;
            existing.ExpenseBudget = budgetDTO.ExpenseBudget;
            existing.StartDateUtc = budgetDTO.StartDate;
            existing.EndDateUtc = budgetDTO.EndDate;

            await budgetRepository.UpdateAsync(existing);
            return Results.Ok();
        })
        .WithName("UpdateBudget")
        .Accepts<BudgetDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        // Get Budgets
        group.MapGet("/budgets", async (
            [FromServices] IRepository<Budget> budgetRepository) =>
        {
            var budgets = await budgetRepository.ListAsync(new GetBudgetsSpec());
            var result = budgets.Select(x => x.MapToDTO());
            return Results.Ok(result);
        })
        .WithName("GetBudgets")
        .Produces<IEnumerable<BudgetDTO>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        // Search Budgets
        group.MapGet("/budgets/search", async (
            [FromQuery] string? searchText,
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromServices] IBudgetSearchRepository budgetSearchRepository) =>
        {
            var pagedResult = await budgetSearchRepository.GetFilteredResultAsync(searchText, pageSize, pageNumber);
            var dto = pagedResult.MapToDTO();
            return Results.Ok(dto);
        })
        .WithName("SearchBudgets")
        .Produces<PagedResultDTO<BudgetDTO>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<bool> UpsertExpense(Expense expense,
            SqliteVectorStore vectorStore)
    {
        try
        {
            // Get and create collection if it doesn't exist.
            var collectionName = "ExpenseSemantics";
            var expenseSemanticCollection = vectorStore.GetCollection<int, ExpenseSemantic>(collectionName);
            await expenseSemanticCollection.EnsureCollectionExistsAsync().ConfigureAwait(false);

            await expenseSemanticCollection.UpsertAsync(expense.ExpenseSemantic);
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }
}
