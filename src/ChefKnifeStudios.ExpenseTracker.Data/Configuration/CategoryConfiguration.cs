using ChefKnifeStudios.ExpenseTracker.Data.Constants;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChefKnifeStudios.ExpenseTracker.Data.Configuration;

internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", DbSchemas.ExpenseTracker);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .HasColumnType("varchar(50)");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}