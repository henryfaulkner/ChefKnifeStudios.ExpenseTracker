﻿using ChefKnifeStudios.ExpenseTracker.MobileApp.Services;
using ChefKnifeStudios.ExpenseTracker.Shared;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

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
    }
}
