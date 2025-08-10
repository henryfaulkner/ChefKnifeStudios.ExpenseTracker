using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Search;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
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
            Categories = model.ExpenseCategories?
                .Where(ec => ec.Category != null)
                .Select(ec => ec.Category!.MapToDTO())
                .ToList() ?? new List<CategoryDTO>(),
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
            ExpenseCategories = dto.Categories?
                .Select(catDto => new ExpenseCategory
                {
                    CategoryId = catDto.Id
                    // ExpenseId will be set by EF when saving, or you can set it if needed
                })
                .ToList() ?? new List<ExpenseCategory>(),
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
            Labels = string.IsNullOrWhiteSpace(model.LabelsJson)
                ? []
                : JsonSerializer.Deserialize<IEnumerable<string>>(model.LabelsJson, Shared.JsonOptions.Get()) ?? [],
            CategoryIds = string.IsNullOrWhiteSpace(model.CategoryIdsJson)
                ? []
                : JsonSerializer.Deserialize<IEnumerable<int>>(model.CategoryIdsJson, Shared.JsonOptions.Get()) ?? [],
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
            CategoryIdsJson = JsonSerializer.Serialize(dto.CategoryIds),
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
            Labels = JsonSerializer.Deserialize<IEnumerable<string>>(model.LabelsJson, Shared.JsonOptions.Get()) ?? [],
            AppId = model.AppId,
            Icon = model.Icon,
        };
    }

    public static Category MapToModel(this CategoryDTO dto)
    {
        return new Category
        {
            Id = dto.Id,
            DisplayName = dto.DisplayName,
            CategoryType = (Data.Enums.CategoryTypes)(int)dto.CategoryType,
            LabelsJson = JsonSerializer.Serialize(dto.Labels),
            AppId = dto.AppId,
            Icon = dto.Icon,
        };
    }
}
