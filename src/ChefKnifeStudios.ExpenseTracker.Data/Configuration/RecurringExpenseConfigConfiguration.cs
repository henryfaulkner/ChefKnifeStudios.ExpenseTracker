using ChefKnifeStudios.ExpenseTracker.Data.Constants;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChefKnifeStudios.ExpenseTracker.Data.Configuration;

public class RecurringExpenseConfigConfiguration : IEntityTypeConfiguration<RecurringExpenseConfig>
{
    public void Configure(EntityTypeBuilder<RecurringExpenseConfig> builder)
    {
        builder.ToTable("RecurringExpenseConfigs", DbSchemas.ExpenseTracker);

        builder.HasKey(x => x.Id);
        builder.Property(e => e.Id)
          .ValueGeneratedOnAdd() 
          .UseIdentityColumn();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LabelsJson)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Cost)
            .IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}