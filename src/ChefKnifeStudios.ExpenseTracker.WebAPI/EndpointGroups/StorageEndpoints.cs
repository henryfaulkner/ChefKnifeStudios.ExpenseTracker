using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.BL;
using ChefKnifeStudios.ExpenseTracker.BL.Services;
using Microsoft.AspNetCore.Mvc;
using ChefKnifeStudios.ExpenseTracker.Shared;

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
            [FromServices] IStorageService storageService,
            [FromServices] IRepository<Budget> budgetRepository,
            HttpContext context) =>
        {
            var appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

            if (string.IsNullOrEmpty(appId))
            {
                return Results.BadRequest("App ID header is required");
            }

            var result = await storageService.AddExpenseAsync(expenseDTO, appId);
            return result ? Results.Ok() : Results.Problem("Failed to add expense");
        })
        .WithName("AddExpense")
        .Accepts<ExpenseDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        // Add Budget
        group.MapPost("/budget", async (
            [FromBody] BudgetDTO budgetDTO,
            [FromServices] IStorageService storageService,
            HttpContext context) =>
        {
            var appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

            if (string.IsNullOrEmpty(appId))
            {
                return Results.BadRequest("App ID header is required");
            }

            var budget = budgetDTO.MapToModel();
            var result = await storageService.AddBudgetAsync(budget, appId);
            return result ? Results.Ok() : Results.Problem("Failed to add budget");
        })
        .WithName("AddBudget")
        .Accepts<BudgetDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        // Update Budget
        group.MapPut("/budget", async (
            [FromBody] BudgetDTO budgetDTO,
            [FromServices] IStorageService storageService,
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

            var result = await storageService.UpdateBudgetAsync(existing);
            return result ? Results.Ok() : Results.Problem("Failed to update budget");
        })
        .WithName("UpdateBudget")
        .Accepts<BudgetDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        // Get Budgets
        group.MapGet("/budgets", async (
            [FromServices] IStorageService storageService) =>
        {
            var result = await storageService.GetBudgetsAsync();
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
            [FromServices] IStorageService storageService) =>
        {
            var pagedResult = await storageService.SearchBudgetsAsync(searchText, pageSize, pageNumber);
            var dto = pagedResult.MapToDTO();
            return Results.Ok(dto);
        })
        .WithName("SearchBudgets")
        .Produces<PagedResultDTO<BudgetDTO>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/recurring-expense", async (
            [FromBody] RecurringExpenseConfigDTO recurringExpenseDTO,
            [FromServices] IStorageService storageService,
            HttpContext context) =>
        {
            var appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

            if (string.IsNullOrEmpty(appId))
            {
                return Results.BadRequest("App ID header is required");
            }

            var recurringExpense = recurringExpenseDTO.MapToModel();
            var result = await storageService.AddRecurringExpenseAsync(recurringExpense, appId);
            return result ? Results.Ok() : Results.Problem("Failed to add recurring expense");
        })
        .WithName("AddRecurringExpense")
        .Accepts<RecurringExpenseConfigDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/process-recurring-expenses", async (
            [FromServices] IStorageService storageService) =>
        {
            await storageService.ProcessRecurringExpensesAsync();
            return Results.Ok();
        })
        .WithName("ProcessRecurringExpenses")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
