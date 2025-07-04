using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChefKnifeStudios.ExpenseTracker.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDataServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(optionsBuilder =>
        {
            optionsBuilder.UseNpgsql(connectionString);
        });

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));

        services.AddTransient<IBudgetSearchRepository, BudgetSearchRepository>();

        return services;
    }
}
