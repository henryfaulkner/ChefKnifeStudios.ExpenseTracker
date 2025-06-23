using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Search;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ChefKnifeStudios.ExpenseTracker.WebAPI;

public static class MappingExtensions
{
    public static ExpenseDTO MapToDTO(this Expense model)
    {
        ExpenseDTO result = new()
        {
            Id = model.Id,
            BudgetId = model.BudgetId,
            Name = model.Name,
            Cost = model.Cost,
            Labels = JsonSerializer.Deserialize<IEnumerable<string>>(model.LabelsJson, Shared.JsonOptions.Get()) ?? [],
            ExpenseSemantic = model.ExpenseSemantic is null ? null
                : new()
                {
                    Id = model.ExpenseSemantic.Id,
                    ExpenseId = model.ExpenseSemantic.ExpenseId,
                    Labels = model.ExpenseSemantic.Labels,
                    SemanticEmbedding = MemoryMarshal.Cast<byte, float>(model.ExpenseSemantic.SemanticEmbedding).ToArray(),
                },
        };
        return result;
    }

    public static Expense MapToModel(this ExpenseDTO dto)
    {
        Expense result = new()
        {
            Id = dto.Id,
            BudgetId = dto.BudgetId,
            Name = dto.Name,
            Cost = dto.Cost,
            LabelsJson = JsonSerializer.Serialize(dto.Labels),
            ExpenseSemantic = dto.ExpenseSemantic is null ? null
            : new()
            {
                Id = dto.ExpenseSemantic.Id,
                ExpenseId = dto.ExpenseSemantic.ExpenseId,
                Labels = dto.ExpenseSemantic.Labels,
                SemanticEmbedding = MemoryMarshal.AsBytes<float>(dto.ExpenseSemantic.SemanticEmbedding.Span).ToArray(),
            },
        };
        return result;
    }

    public static BudgetDTO MapToDTO(this Budget model)
    {
        List<ExpenseDTO> expenseDTOs = new ();
        if (model.Expenses != null) 
            foreach (var expense in model.Expenses) 
                expenseDTOs.Add(expense.MapToDTO());

        BudgetDTO result = new ()
        {
            Id = model.Id,
            Name = model.Name,
            ExpenseBudget = model.ExpenseBudget,
            StartDate = model.StartDateUtc, // convert from UTC to Local
            EndDate = model.EndDateUtc, // convert from UTC to Local
            ExpenseDTOs = expenseDTOs,
        };
        
        return result;
    }

    public static Budget MapToModel(this BudgetDTO dto)
    {
        List<Expense> expenses = new ();
        if (dto.ExpenseDTOs != null) 
            foreach (var expenseDTO in dto.ExpenseDTOs) 
                expenses.Add(expenseDTO.MapToModel());

        Budget result = new () 
        {
            Id = dto.Id,
            Name = dto.Name,
            ExpenseBudget = dto.ExpenseBudget,
            StartDateUtc = dto.StartDate, // convert from Local to UTC
            EndDateUtc = dto.EndDate, // convert from Local to UTC
            Expenses = expenses,
        };

        return result;
    }

    public static PagedResultDTO<BudgetDTO> MapToDTO(this PagedResult<Budget> model)
    {
        List<BudgetDTO> records = new ();
        if (model.Records != null)
            foreach (var budget in model.Records)
                records.Add(budget.MapToDTO());

        PagedResultDTO<BudgetDTO> result = new ()
        {
            TotalRecords = model.TotalRecords,
            PageSize = model.PageSize,
            PageNumber = model.PageNumber,
            Records = records,
        };

        return result;
    }

    public static RecurringExpenseConfigDTO MapToDTO(this RecurringExpenseConfig model)
    {
        RecurringExpenseConfigDTO result = new()
        {
            Id = model.Id,
            Name = model.Name,
            Cost = model.Cost,
            Labels = JsonSerializer.Deserialize<IEnumerable<string>>(model.LabelsJson, Shared.JsonOptions.Get()) ?? [],
        };
        return result;
    }

    public static RecurringExpenseConfig MapToModel(this RecurringExpenseConfigDTO dto)
    {
        RecurringExpenseConfig result = new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Cost = dto.Cost,
            LabelsJson = JsonSerializer.Serialize(dto.Labels),
        };
        return result;
    }
}
