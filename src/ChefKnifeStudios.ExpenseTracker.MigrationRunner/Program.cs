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
            optionsBuilder.UseNpgsql(connectionString);
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Run migrations
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Console.WriteLine("Applying migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("Migrations applied successfully.");
    }
}