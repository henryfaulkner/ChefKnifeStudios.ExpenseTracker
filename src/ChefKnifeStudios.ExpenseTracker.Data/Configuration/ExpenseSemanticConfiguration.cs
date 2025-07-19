using ChefKnifeStudios.ExpenseTracker.Data.Constants;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChefKnifeStudios.ExpenseTracker.Data.Configuration;

public class ExpenseSemanticConfiguration : IEntityTypeConfiguration<ExpenseSemantic>
{
    public void Configure(EntityTypeBuilder<ExpenseSemantic> builder)
    {
        builder.ToTable("ExpenseSemantics", DbSchemas.ExpenseTracker);

        builder.HasKey(x => x.Id);
        builder.Property(e => e.Id)
          .ValueGeneratedOnAdd()
          .UseIdentityColumn();

        builder.HasOne(x => x.Expense)
            .WithOne(x => x.ExpenseSemantic)
            .HasForeignKey<ExpenseSemantic>(x => x.ExpenseId);

        // Ignore not-mapped properties
        builder.Ignore(x => x.SemanticEmbeddingVectors);
        builder.Ignore(x => x.Score);
    }
}