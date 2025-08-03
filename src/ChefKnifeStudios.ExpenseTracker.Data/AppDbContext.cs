using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace ChefKnifeStudios.ExpenseTracker.Data;

public class AppDbContext : DbContext
{   
	public DbSet<Budget> Budgets { get; set; }
	public DbSet<Category> Categories { get; set; }
	public DbSet<CategorySemantic> CategorySemantics { get; set; }
	public DbSet<Expense> Expenses { get; set; }
	public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
	public DbSet<ExpenseSemantic> ExpenseSemantics { get; set; }
	public DbSet<RecurringExpenseConfig> RecurringExpenseConfigs { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

	public override int SaveChanges()
	{
		AddAuditInfo();
		return base.SaveChanges();
	}

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		AddAuditInfo();
		return await base.SaveChangesAsync(cancellationToken);
	}

    void AddAuditInfo()
	{
		var entities = ChangeTracker.Entries<BaseEntity>().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
		var utcNow = DateTime.UtcNow;
		
		foreach (var entity in entities)
		{
			if (entity.State == EntityState.Added)
			{
				entity.Entity.CreatedOnUtc = utcNow;
				entity.Entity.ModifiedOnUtc = null;
                entity.Entity.IsDeleted = false;
			}

			if (entity.State == EntityState.Modified)
			{
				entity.Entity.ModifiedOnUtc = utcNow;
			}
		}
	}
}
