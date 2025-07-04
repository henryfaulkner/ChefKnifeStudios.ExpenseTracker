using ChefKnifeStudios.ExpenseTracker.BL;
using ChefKnifeStudios.ExpenseTracker.BL.Models;
using ChefKnifeStudios.ExpenseTracker.BL.Services;
using ChefKnifeStudios.ExpenseTracker.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddLogging(logging =>
    {
        logging.AddConsole(); // This is CRITICAL for logs to appear in Azure Log Stream
        logging.SetMinimumLevel(LogLevel.Debug); // Adjust as needed
    });

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
if (appSettings == null) throw new ApplicationException("AppSettings are not configured correctly.");

builder.Services
    .RegisterDataServices(builder.Configuration)
    .AddTransient<IStorageService, StorageService>()
    .AddTransient<ISemanticService, SemanticService>()
    .AddPostgresVectorStore(_ => builder.Configuration.GetConnectionString("ExpenseTrackerDB")!); //https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/postgres-connector?pivots=programming-language-csharp

builder.Services
    .AddKernel()
    .ConfigureSemanticKernel(appSettings);

// Register OpenAIClient with the API key and endpoint
builder.Services.AddSingleton<OpenAIClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var openAiConfig = configuration.GetSection("OpenAI");
    var apiKey = openAiConfig.GetValue<string>("Key")
                 ?? throw new InvalidOperationException("OpenAI API Key is missing.");

    // Use the API key to configure the client
    return new OpenAIClient(apiKey);
});

builder.Build().Run();
