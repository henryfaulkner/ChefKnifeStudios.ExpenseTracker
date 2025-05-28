using ChefKnifeStudios.ExpenseTracker.Data;
using ChefKnifeStudios.ExpenseTracker.MobileApp.Services;
using ChefKnifeStudios.ExpenseTracker.Shared;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MatBlazor;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.Services.AddMatBlazor();

            #if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
            #endif

            builder.Services.RegisterDataServices(builder.Configuration);

            builder.Services.AddTransient<IStorageService, StorageService>();
            builder.Services.AddSingleton<IEventNotificationService, EventNotificationService>();

            builder.Services.RegisterViewModels(builder.Configuration);

            return builder.Build();
        }
    }
}
