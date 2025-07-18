﻿using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using MatBlazor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

#if WINDOWS10_0_17763_0_OR_GREATER
using CommunityToolkit.Maui.Maps;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Platform;
#endif

namespace ChefKnifeStudios.ExpenseTracker.MobileApp;

public static class CommunityToolkitExtensions
{
    public static void RegisterCommunityToolkit(this MauiAppBuilder builder)
    {
        builder
                .UseMauiApp<App>()

#if DEBUG
                .UseMauiCommunityToolkit(static options =>
                {
                    options.SetShouldEnableSnackbarOnWindows(true);
                })
#else
				.UseMauiCommunityToolkit(static options =>
				{
					options.SetShouldEnableSnackbarOnWindows(true);
					options.SetShouldSuppressExceptionsInConverters(true);
					options.SetShouldSuppressExceptionsInBehaviors(true);
					options.SetShouldSuppressExceptionsInAnimations(true);
				})
#endif
                .UseMauiCommunityToolkitMarkup()
                .UseMauiCommunityToolkitCamera()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureMauiHandlers(static handlers =>
                {
#if IOS || MACCATALYST
                    handlers.AddHandler<CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
                    handlers.AddHandler<CarouselView, Microsoft.Maui.Controls.Handlers.Items2.CarouselViewHandler2>();
#endif
                })

#if WINDOWS
				.UseMauiCommunityToolkitMaps("KEY") // You should add your own key here from https://bingmapsportal.com
#else
                .UseMauiMaps()
#endif
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
    }
}
