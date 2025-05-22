using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace ChefKnifeStudios.ExpenseTracker.Data;

public class AppDbContext : DbContext
{
    readonly IConfiguration _configuration;
    
    public AppDbContext(
        DbContextOptions<AppDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration.GetConnectionString("ExpenseTrackerDB");
        optionsBuilder.UseSqlite(connectionString);
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
