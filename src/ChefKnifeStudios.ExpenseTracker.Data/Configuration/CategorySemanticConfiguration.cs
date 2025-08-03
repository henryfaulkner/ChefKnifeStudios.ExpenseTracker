using ChefKnifeStudios.ExpenseTracker.Data.Constants;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChefKnifeStudios.ExpenseTracker.Data.Configuration;

public class CategorySemanticConfiguration : IEntityTypeConfiguration<CategorySemantic>
{
    public void Configure(EntityTypeBuilder<CategorySemantic> builder)
    {
        builder.ToTable("CategorySemantics", DbSchemas.ExpenseTracker);

        builder.HasKey(x => x.Id);
        builder.Property(e => e.Id)
          .ValueGeneratedOnAdd()
          .UseIdentityColumn();

        builder.HasOne(x => x.Category)
            .WithOne(x => x.CategorySemantic)
            .HasForeignKey<CategorySemantic>(x => x.CategoryId);

        // Ignore not-mapped properties
        builder.Ignore(x => x.SemanticEmbeddingVectors);
        builder.Ignore(x => x.Score);
    }
}