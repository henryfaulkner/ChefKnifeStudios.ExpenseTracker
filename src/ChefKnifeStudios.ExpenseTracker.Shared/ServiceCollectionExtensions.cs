using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.Extensions.Configuration;

namespace ChefKnifeStudios.ExpenseTracker.Shared;

public static class ServiceCollectionExtensions
{
    public static void RegisterViewModels(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ISearchViewModel, SearchViewModel>();
    }
}
