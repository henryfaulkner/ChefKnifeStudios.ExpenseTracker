using ChefKnifeStudios.ExpenseTracker.Data.Constants;
using ChefKnifeStudios.ExpenseTracker.Data.Models.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChefKnifeStudios.ExpenseTracker.Data.Configuration;

public class BudgetWithExpensesConfiguration : IEntityTypeConfiguration<BudgetWithExpenses.ViewResult>
{
    public void Configure(EntityTypeBuilder<BudgetWithExpenses.ViewResult> builder)
    {
        builder.ToView("BudgetWithExpenses", DbSchemas.ExpenseTracker);

        builder.HasKey(x => x.BudgetId);
    }
}
