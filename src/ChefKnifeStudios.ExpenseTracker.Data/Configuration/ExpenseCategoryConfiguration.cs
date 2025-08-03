using ChefKnifeStudios.ExpenseTracker.Data.Constants;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChefKnifeStudios.ExpenseTracker.Data.Configuration;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("ExpenseCategories", DbSchemas.ExpenseTracker);

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Expense)
            .WithMany(x => x.ExpenseCategories)
            .HasForeignKey(x => x.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.ExpenseCategories)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ExpenseId, x.CategoryId })
            .IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}