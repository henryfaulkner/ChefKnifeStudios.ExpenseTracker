using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ChefKnifeStudios.ExpenseTracker.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // EF Core migrations bundle passes the connection string as the first argument (not --connection=)
        var connectionString = args.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.Trim().EndsWith(".dll"))
        {
            optionsBuilder.UseNpgsql(connectionString);
            return new AppDbContext(optionsBuilder.Options, connectionString);
        }

        // Fallback to appsettings.json if no connection string argument is provided
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionStringFromConfig = configuration.GetConnectionString("ExpenseTrackerDB");
        optionsBuilder.UseNpgsql(connectionStringFromConfig);

        return new AppDbContext(optionsBuilder.Options, configuration);
    }
}
