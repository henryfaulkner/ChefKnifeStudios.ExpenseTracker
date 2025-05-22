set ansi_nulls on
go
set quoted_identifier on
go
create or alter view [ExpenseTracker].[BudgetWithExpenses]
as
select 
    b.Id as BudgetId,
    coalesce(sum(e.Cost), 0) as TotalExpenseCost,
    group_concat(e.Id) as ExpenseIds -- Collect all Expense IDs as a comma-separated string
from 
    Budgets b
left join 
    Expenses e
on 
    b.CreatedOnUtc between e.StartDateUtc and e.EndDateUtc
where 
    b.IsDeleted = 0 and (e.IsDeleted = 0 or e.Id is null) -- Filter out deleted records
group by 
    b.Id;
go
