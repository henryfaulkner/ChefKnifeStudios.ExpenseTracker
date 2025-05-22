using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Models.Views;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp;

public static class MappingExtensions
{
    public static ExpenseDTO MapToDTO(this Expense model)
    {
        ExpenseDTO result = new ()
        { 
            Id = model.Id,
            Name = model.Name,
            Cost = model.Cost,
        };
        return result;
    }

    public static Expense MapToModel(this ExpenseDTO dto)
    {
        Expense result = new ()
        { 
            Id = dto.Id,
            Name = dto.Name,
            Cost = dto.Cost,
        };
        return result;
    }

    public static BudgetDTO MapToDTO(this Budget model)
    {
        BudgetDTO result = new ()
        {
            Id = model.Id,
            Name = model.Name,
            ExpenseBudget = model.ExpenseBudget,
            StartDate = model.StartDateUtc, // convert from UTC to Local
            EndDate = model.EndDateUtc, // convert from UTC to Local
        };
        return result;
    }

    public static Budget MapToModel(this BudgetDTO dto)
    {
        Budget result = new () 
        {
            Id = dto.Id,
            Name = dto.Name,
            ExpenseBudget = dto.ExpenseBudget,
            StartDateUtc = dto.StartDate, // convert from Local to UTC
            EndDateUtc = dto.EndDate, // convert from Local to UTC
        };
        return result;
    }

    public static BudgetWithExpensesDTO MapToDTO(this BudgetWithExpenses model)
    {
        BudgetWithExpensesDTO result;
        BudgetDTO budgetDTO = MapToDTO(model.Budget);
        List<ExpenseDTO> expenseDTOs = new ();
        foreach (var expense in model.Expenses) expenseDTOs.Add(MapToDTO(expense));

        result = new ()
        { 
            Budget = budgetDTO,
            TotalExpenseCost = model.TotalExpenseCost,
            Expenses = expenseDTOs,
        };
        return result;
    }

    public static BudgetWithExpenses MapToModel(this BudgetWithExpensesDTO dto)
    {
        BudgetWithExpenses result;
        Budget budget = MapToModel(dto.Budget);
        List<Expense> expenses = new ();
        foreach (var expenseDTO in dto.Expenses) expenses.Add(MapToModel(expenseDTO));

        result = new ()
        { 
            Budget = budget,
            TotalExpenseCost = dto.TotalExpenseCost,
            Expenses = expenses,
        };
        return result;
    }
}
