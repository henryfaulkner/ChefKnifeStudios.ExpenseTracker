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
                .UseSentry(options =>
                {
                    options.Dsn = "https://4b5fffa8fb967e46632adfed5c4e7ea0@o4509406577098752.ingest.us.sentry.io/4509406586863616";
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.Services.AddMatBlazor();
            builder.Services.AddMatToaster(config =>
            {
                config.Position = MatToastPosition.BottomLeft;
                config.PreventDuplicates = true;
                config.NewestOnTop = true;
                config.VisibleStateDuration = 3000;
                config.ShowCloseButton = true;
                config.ShowProgressBar = true;
                config.MaximumOpacity = 100;
                config.ShowTransitionDuration = 300;
                config.VisibleStateDuration = 4000;
                config.HideTransitionDuration = 300;
                config.RequireInteraction = false;
            });

            #if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
            #endif

            builder.Services.AddTransient<IStorageService, StorageService>();
            builder.Services.AddTransient<ISemanticService, SemanticService>();
            builder.Services.AddTransient<ICameraService, CameraService>();
            builder.Services.AddTransient<IToastService, ToastService>();
            builder.Services.AddTransient<ICommonJsInteropService, CommonJsInteropService>();
            builder.Services.AddSingleton<IEventNotificationService, EventNotificationService>();
            builder.Services.AddScoped<HttpClient>();

            builder.Services.RegisterViewModels(builder.Configuration);

            return builder.Build();
        }
    }
}
