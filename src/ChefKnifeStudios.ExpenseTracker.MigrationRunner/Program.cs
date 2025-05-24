using System;
using ChefKnifeStudios.ExpenseTracker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.Json;

namespace ChefKnifeStudios.ExpenseTracker.MigrationRunner;

internal class Program
{
    static void Main(string[] args)
    {
        System.Diagnostics.Debugger.Launch();
        Console.WriteLine("Starting Migration Runner...");

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Setup DI
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDbContext<AppDbContext>(optionsBuilder =>
        {
            var connectionString = configuration.GetConnectionString("ExpenseTrackerDB");
            var dbPath = GetDatabasePathFromConnectionString(connectionString);
            if (!string.IsNullOrEmpty(dbPath))
            {
                var dbDir = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }
            }
            optionsBuilder.UseSqlite(connectionString);
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Run migrations
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Console.WriteLine("Applying migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("Migrations applied successfully.");
    }

    static string? GetDatabasePathFromConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return null;

        // Typical SQLite connection string: Data Source=app_database.db
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 && kv[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase))
            {
                return kv[1].Trim();
            }
        }
        return null;
    }
}
