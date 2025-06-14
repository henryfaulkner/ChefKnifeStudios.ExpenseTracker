﻿using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Data.Search;
using ChefKnifeStudios.ExpenseTracker.Data.Specifications;
using Microsoft.AspNetCore.Mvc;

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
            [FromServices] IRepository<Expense> expenseRepository) =>
        {
            var expense = expenseDTO.MapToModel();
            await expenseRepository.AddAsync(expense);
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
}
