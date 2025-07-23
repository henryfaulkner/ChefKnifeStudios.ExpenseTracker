using ChefKnifeStudios.ExpenseTracker.Data.Constants;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChefKnifeStudios.ExpenseTracker.Data.Configuration;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable($"{nameof(Expense)}s", DbSchemas.ExpenseTracker);

        builder.HasKey(x => x.Id);
        builder.Property(e => e.Id)
          .ValueGeneratedOnAdd()
          .UseIdentityColumn();

        builder.HasOne(x => x.Budget)
            .WithMany(x => x.Expenses);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}