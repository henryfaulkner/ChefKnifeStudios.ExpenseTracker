using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace ChefKnifeStudios.ExpenseTracker.Data;

public class AppDbContext : DbContext
{
    readonly IConfiguration? _configuration;
    readonly string? _explicitConnectionString;

    public DbSet<Budget> Budgets { get; set; }
	public DbSet<Expense> Expenses { get; set; }
	public DbSet<ExpenseSemantic> ExpenseSemantics { get; set; }
	public DbSet<RecurringExpenseConfig> RecurringExpenseConfigs { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, string connectionString)
        : base(options)
    {
        _explicitConnectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Use explicit connection string if provided
            if (!string.IsNullOrEmpty(_explicitConnectionString))
            {
                optionsBuilder.UseNpgsql(_explicitConnectionString);
            }
            // Fallback to configuration if available
            else if (_configuration != null)
            {
                var connectionString = _configuration.GetConnectionString("ExpenseTrackerDB");
                optionsBuilder.UseNpgsql(connectionString);
            }
        }
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
