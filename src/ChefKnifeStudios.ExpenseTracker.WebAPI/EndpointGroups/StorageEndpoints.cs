using ChefKnifeStudios.ExpenseTracker.BL;
using ChefKnifeStudios.ExpenseTracker.BL.Services;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.Shared;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace ChefKnifeStudios.ExpenseTracker.WebAPI.EndpointGroups;

public static class StorageEndpoints
{
    public static IEndpointRouteBuilder MapStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/storage")
            .WithName("Storage");

        group.MapPost("/expense", async (
            [FromBody] ExpenseDTO expenseDTO,
            [FromServices] IStorageService storageService,
            [FromServices] IRepository<Budget> budgetRepository,
            [FromServices] ILoggerFactory loggerFactory,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(StorageEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                var result = await storageService.AddExpenseAsync(expenseDTO, Guid.Parse(appId), cancellationToken);
                return result ? Results.Ok() : Results.Problem("Failed to add expense");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception in AddExpense endpoint. AppId: {AppId}", appId);
                return Results.Problem("An unexpected error occurred.", statusCode: 500);
            }
        })
        .WithName("AddExpense")
        .Accepts<ExpenseDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPatch("/expense/{id}/price", async (
            int id,
            [FromBody] decimal cost,
            [FromServices] IStorageService storageService,
            [FromServices] ILoggerFactory loggerFactory,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(StorageEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                var result = await storageService.UpdateExpenseCostAsync(id, cost, Guid.Parse(appId), cancellationToken);
                return result ? Results.Ok() : Results.Problem("Failed to update expense's cost");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception in UpdateExpenseCost endpoint. AppId: {AppId}", appId);
                return Results.Problem("An unexpected error occurred.", statusCode: 500);
            }
        })
        .WithName("UpdateExpenseCost")
        .Accepts<bool>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPatch("/expense/{id}/delete", async (
            int id,
            [FromServices] IStorageService storageService,
            [FromServices] ILoggerFactory loggerFactory,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
                {
                    var logger = loggerFactory.CreateLogger(nameof(StorageEndpoints));
                    string? appId = null;
                    try
                    {
                        appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                        if (string.IsNullOrEmpty(appId))
                        {
                            return Results.BadRequest("App ID header is required");
                        }

                        var result = await storageService.DeleteExpenseCostAsync(id, Guid.Parse(appId), cancellationToken);
                        return result ? Results.Ok() : Results.Problem("Failed to mark expense as deleted");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Exception in DeleteExpenseCost endpoint. AppId: {AppId}", appId);
                        return Results.Problem("An unexpected error occurred.", statusCode: 500);
                    }
                })
        .WithName("DeleteExpense")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/budget", async (
            [FromBody] BudgetDTO budgetDTO,
            [FromServices] IStorageService storageService,
            [FromServices] ILoggerFactory loggerFactory,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(StorageEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                var budget = budgetDTO.MapToModel();
                var result = await storageService.AddBudgetAsync(budget, Guid.Parse(appId), cancellationToken);
                return result ? Results.Ok() : Results.Problem("Failed to add budget");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception in AddBudget endpoint. AppId: {AppId}", appId);
                return Results.Problem("An unexpected error occurred.", statusCode: 500);
            }
        })
        .WithName("AddBudget")
        .Accepts<BudgetDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPut("/budget", async (
            [FromBody] BudgetDTO budgetDTO,
            [FromServices] IStorageService storageService,
            [FromServices] IRepository<Budget> budgetRepository,
            [FromServices] ILoggerFactory loggerFactory,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(StorageEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                var existing = await budgetRepository.GetByIdAsync(budgetDTO.Id);
                if (existing == null)
                    return Results.NotFound();

                existing.Name = budgetDTO.Name;
                existing.ExpenseBudget = budgetDTO.ExpenseBudget;
                existing.StartDateUtc = budgetDTO.StartDate;
                existing.EndDateUtc = budgetDTO.EndDate;

                var result = await storageService.UpdateBudgetAsync(existing, Guid.Parse(appId), cancellationToken);
                return result ? Results.Ok() : Results.Problem("Failed to update budget");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception in UpdateBudget endpoint. AppId: {AppId}", appId);
                return Results.Problem("An unexpected error occurred.", statusCode: 500);
            }
        })
        .WithName("UpdateBudget")
        .Accepts<BudgetDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/budgets", async (
            [FromServices] IStorageService storageService,
            [FromServices] ILoggerFactory loggerFactory,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(StorageEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                var result = await storageService.GetBudgetsAsync(Guid.Parse(appId));
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception in GetBudgets endpoint. AppId: {AppId}", appId);
                return Results.Problem("An unexpected error occurred.", statusCode: 500);
            }
        })
        .WithName("GetBudgets")
        .Produces<IEnumerable<BudgetDTO>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/budgets/search", async (
            [FromQuery] string? searchText,
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromServices] IStorageService storageService,
            [FromServices] ILoggerFactory loggerFactory,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(StorageEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                var pagedResult = await storageService.SearchBudgetsAsync(searchText, pageSize, pageNumber, Guid.Parse(appId), cancellationToken);
                var dto = pagedResult.MapToDTO();
                return Results.Ok(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception in SearchBudgets endpoint. AppId: {AppId}", appId);
                return Results.Problem("An unexpected error occurred.", statusCode: 500);
            }
        })
        .WithName("SearchBudgets")
        .Produces<PagedResultDTO<BudgetDTO>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/recurring-expense", async (
            [FromServices] IStorageService storageService,
            [FromServices] ILoggerFactory loggerFactory,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(StorageEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                var result = await storageService.GetRecurringExpensesAsync(Guid.Parse(appId), cancellationToken);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception in GetRecurringExpenses endpoint. AppId: {AppId}", appId);
                return Results.Problem("An unexpected error occurred.", statusCode: 500);
            }
        })
        .WithName("GetRecurringExpenses")
        .Accepts<IEnumerable<RecurringExpenseConfigDTO>>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/recurring-expense", async (
            [FromBody] RecurringExpenseConfigDTO recurringExpenseDTO,
            [FromServices] IStorageService storageService,
            [FromServices] ILoggerFactory loggerFactory,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(StorageEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                var recurringExpense = recurringExpenseDTO.MapToModel();
                var result = await storageService.AddRecurringExpenseAsync(recurringExpense, Guid.Parse(appId), cancellationToken);
                return result ? Results.Ok() : Results.Problem("Failed to add recurring expense");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception in AddRecurringExpense endpoint. AppId: {AppId}", appId);
                return Results.Problem("An unexpected error occurred.", statusCode: 500);
            }
        })
        .WithName("AddRecurringExpense")
        .Accepts<RecurringExpenseConfigDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPatch("/recurring-expense/{id}/delete", async (
            int id,
            [FromServices] IStorageService storageService,
            [FromServices] ILoggerFactory loggerFactory,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(StorageEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                var result = await storageService.DeleteRecurringExpenseAsync(id, Guid.Parse(appId), cancellationToken);
                return result ? Results.Ok() : Results.Problem("Failed to delete recurring expense");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception in UpdateExpenseCost endpoint. AppId: {AppId}", appId);
                return Results.Problem("An unexpected error occurred.", statusCode: 500);
            }
        })
        .WithName("DeleteRecurringExpense")
        .Accepts<bool>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/process-recurring-expenses", async (
            [FromServices] IStorageService storageService,
            [FromServices] ILoggerFactory loggerFactory,
            HttpContext context,
            CancellationToken cancellationToken = default) =>
        {
            var logger = loggerFactory.CreateLogger(nameof(StorageEndpoints));
            string? appId = null;
            try
            {
                appId = context.Request.Headers[Constants.AppIdHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(appId))
                {
                    return Results.BadRequest("App ID header is required");
                }

                await storageService.ProcessRecurringExpensesAsync(cancellationToken);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception in ProcessRecurringExpenses endpoint. AppId: {AppId}", appId);
                return Results.Problem("An unexpected error occurred.", statusCode: 500);
            }
        })
        .WithName("ProcessRecurringExpenses")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}