using ChefKnifeStudios.ExpenseTracker.MobileApp.Services;
using ChefKnifeStudios.ExpenseTracker.Shared;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.UseEmbeddedConfiguration();

            builder.RegisterCommunityToolkit();

            builder.Services.AddTransient<IAppSessionService, AppSessionService>();
            builder.Services.AddTransient<ICameraService, CameraService>();
            builder.Services.AddSingleton<IMicrophoneService, MicrophoneService>();
            builder.Services.AddTransient<IToastService, ToastService>();
            builder.Services.AddTransient<ICommonJsInteropService, CommonJsInteropService>();
            builder.Services.AddSingleton<IEventNotificationService, EventNotificationService>();
            builder.Services.AddSingleton<ISpeechToText, OfflineSpeechToTextImplementation>();

            builder.Services.AddTransient<HttpHeaderHandler>();
            builder.Services
                .AddTransient<IStorageService, StorageService>()
                .AddHttpClient("ExpenseTrackerAPI", client =>
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "ExpenseTracker/1.0");
                    client.Timeout = TimeSpan.FromSeconds(30);
                })
                .AddHttpMessageHandler<HttpHeaderHandler>();
            builder.Services
                .AddTransient<ISemanticService, SemanticService>()
                .AddHttpClient("ExpenseTrackerAPI", client =>
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "ExpenseTracker/1.0");
                    client.Timeout = TimeSpan.FromSeconds(30);
                })
                .AddHttpMessageHandler<HttpHeaderHandler>();


            builder.Services.RegisterViewModels(builder.Configuration);

            return builder.Build();
        }

        static void UseEmbeddedConfiguration(this MauiAppBuilder builder)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();

                // Debug: Log all resource names to see what's available
                System.Diagnostics.Debug.WriteLine("Available embedded resources:");
                foreach (var name in resourceNames)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {name}");
                }

                // Try different possible resource names
                var possibleNames = new[]
                {
                    "appsettings.json",
                    "ChefKnifeStudios.ExpenseTracker.MobileApp.appsettings.json",
                    resourceNames.FirstOrDefault(x => x.EndsWith("appsettings.json", StringComparison.OrdinalIgnoreCase))
                };

                string appSettingsResourceName = null;
                foreach (var possibleName in possibleNames.Where(x => !string.IsNullOrEmpty(x)))
                {
                    if (resourceNames.Contains(possibleName))
                    {
                        appSettingsResourceName = possibleName;
                        break;
                    }
                }

                if (appSettingsResourceName == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: appsettings.json not found in embedded resources");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Found appsettings.json as: {appSettingsResourceName}");

                using var stream = assembly.GetManifestResourceStream(appSettingsResourceName);

                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Could not load embedded appsettings.json stream");
                    return;
                }

                // Alternative approach: Read to string first, then add
                using var reader = new StreamReader(stream);
                var jsonContent = reader.ReadToEnd();
                System.Diagnostics.Debug.WriteLine($"JSON content length: {jsonContent.Length}");

                // Reset stream position and add to configuration
                stream.Position = 0;
                builder.Configuration.AddJsonStream(stream);

                System.Diagnostics.Debug.WriteLine("Successfully loaded embedded appsettings.json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR loading embedded configuration: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
