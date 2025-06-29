using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ChefKnifeStudios.ExpenseTracker.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Debug: Log the args received
        Console.WriteLine("AppDbContextFactory: args = [" + string.Join(", ", args) + "]");

        // EF Core migrations bundle passes the connection string as the first argument (not --connection=)
        var connectionString = args.FirstOrDefault();

        // Debug: Log the connection string candidate
        Console.WriteLine("AppDbContextFactory: connectionString candidate = " + (connectionString ?? "<null>"));

        if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.Trim().EndsWith(".dll"))
        {
            Console.WriteLine("AppDbContextFactory: Using connection string from args.");
            optionsBuilder.UseNpgsql(connectionString);
            return new AppDbContext(optionsBuilder.Options, connectionString);
        }

        Console.WriteLine("AppDbContextFactory: Falling back to appsettings.json.");

        // Fallback to appsettings.json if no connection string argument is provided
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionStringFromConfig = configuration.GetConnectionString("ExpenseTrackerDB");
        Console.WriteLine("AppDbContextFactory: connectionString from config = " + (connectionStringFromConfig ?? "<null>"));

        optionsBuilder.UseNpgsql(connectionStringFromConfig);

        return new AppDbContext(optionsBuilder.Options, configuration);
    }
}
