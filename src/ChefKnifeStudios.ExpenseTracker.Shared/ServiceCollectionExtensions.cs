using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChefKnifeStudios.ExpenseTracker.Shared;

public static class ServiceCollectionExtensions
{
    public static void RegisterViewModels(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISearchViewModel, SearchViewModel>();
        services.AddScoped<IExpenseViewModel, ExpenseViewModel>();
        services.AddScoped<IRecurringExpenseViewModel, RecurringExpenseViewModel>();
    }
}
