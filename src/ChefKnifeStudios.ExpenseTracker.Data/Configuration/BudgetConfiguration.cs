using ChefKnifeStudios.ExpenseTracker.Data.Constants;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChefKnifeStudios.ExpenseTracker.Data.Configuration;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets", DbSchemas.ExpenseTracker);

        builder.HasKey(x => x.Id);
        builder.Property(e => e.Id)
          .ValueGeneratedOnAdd()
          .UseIdentityColumn();

        builder.HasMany(x => x.Expenses)
            .WithOne(x => x.Budget);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}