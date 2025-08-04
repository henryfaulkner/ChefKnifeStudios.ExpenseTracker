using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Search;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ChefKnifeStudios.ExpenseTracker.BL;

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
            IsRecurring = model.IsRecurring,
            Budget = model.Budget is null ? null
                : new()
                {
                    Name = model.Budget.Name,
                    ExpenseBudget = model.Budget.ExpenseBudget,
                    StartDate = DateTime.SpecifyKind(model.Budget.StartDateUtc, DateTimeKind.Utc).ToLocalTime(),
                    EndDate = DateTime.SpecifyKind(model.Budget.EndDateUtc, DateTimeKind.Utc).ToLocalTime(),
                },
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
            IsRecurring = dto.IsRecurring,
            Budget = dto.Budget is null ? null
                : new()
                {
                    Name = dto.Budget.Name,
                    ExpenseBudget = dto.Budget.ExpenseBudget,
                    StartDateUtc = dto.Budget.StartDate.Kind == DateTimeKind.Utc ? dto.Budget.StartDate : dto.Budget.StartDate.ToUniversalTime(),
                    EndDateUtc = dto.Budget.EndDate.Kind == DateTimeKind.Utc ? dto.Budget.EndDate : dto.Budget.EndDate.ToUniversalTime(),
                },
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
            StartDate = DateTime.SpecifyKind(model.StartDateUtc, DateTimeKind.Utc).ToLocalTime(),
            EndDate = DateTime.SpecifyKind(model.EndDateUtc, DateTimeKind.Utc).ToLocalTime(),
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
            StartDateUtc = dto.StartDate.Kind == DateTimeKind.Utc ? dto.StartDate : dto.StartDate.ToUniversalTime(),
            EndDateUtc = dto.EndDate.Kind == DateTimeKind.Utc ? dto.EndDate : dto.EndDate.ToUniversalTime(),
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

    public static CategoryDTO MapToDTO(this Category model)
    {
        return new CategoryDTO
        {
            Id = model.Id,
            DisplayName = model.DisplayName,
            CategoryType = (Shared.Enums.CategoryTypes)(int)model.CategoryType,
            Labels = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<string>>(model.LabelsJson, Shared.JsonOptions.Get()) ?? [],
            AppId = model.AppId,
            CategorySemantic = model.CategorySemantic is null ? null : new CategorySemanticDTO
            {
                Id = model.CategorySemantic.Id,
                CategoryId = model.CategorySemantic.CategoryId,
                Labels = model.CategorySemantic.Labels,
                SemanticEmbedding = model.CategorySemantic.SemanticEmbedding is null
                    ? ReadOnlyMemory<float>.Empty
                    : System.Runtime.InteropServices.MemoryMarshal.Cast<byte, float>(model.CategorySemantic.SemanticEmbedding).ToArray()
            }
        };
    }

    public static Category MapToModel(this CategoryDTO dto)
    {
        return new Category
        {
            Id = dto.Id,
            DisplayName = dto.DisplayName,
            CategoryType = (Data.Enums.CategoryTypes)(int)dto.CategoryType,
            LabelsJson = System.Text.Json.JsonSerializer.Serialize(dto.Labels),
            AppId = dto.AppId,
            CategorySemantic = dto.CategorySemantic is null ? null : new CategorySemantic
            {
                Id = dto.CategorySemantic.Id,
                CategoryId = dto.CategorySemantic.CategoryId,
                Labels = dto.CategorySemantic.Labels,
                SemanticEmbedding = dto.CategorySemantic.SemanticEmbedding.IsEmpty
                    ? null
                    : System.Runtime.InteropServices.MemoryMarshal.AsBytes<float>(dto.CategorySemantic.SemanticEmbedding.Span).ToArray()
            }
        };
    }
}
