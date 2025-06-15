using Azure;
using Azure.AI.DocumentIntelligence;
using ChefKnifeStudios.ExpenseTracker.Data;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.WebAPI;
using ChefKnifeStudios.ExpenseTracker.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI;
using OpenAI.VectorStores;
using Scalar.AspNetCore;
using System.Globalization;
using Microsoft.SemanticKernel.Data;
using OpenAI;
using System.Text.Json;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using System.Collections;
using Microsoft.Extensions.Logging;
using ChefKnifeStudios.ExpenseTracker.WebAPI.EndpointGroups;
using ChefKnifeStudios.ExpenseTracker.Data.Models;


var builder = WebApplication.CreateBuilder(args);

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
if (appSettings == null) throw new ApplicationException("AppSettings are not configured correctly.");

builder.Services
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddCors()
    .AddHttpClient()
    .AddTransient<IHttpService, HttpService>()
    .RegisterDataServices(builder.Configuration)
    .AddSqliteVectorStore(_ => builder.Configuration.GetConnectionString("ExpenseTrackerDB"));

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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi()
    .AllowAnonymous();

app.MapScalarApiReference(options =>
{
    options.HiddenClients = true;
    options
        .WithTitle("Overcast.CustomerPortal.Client.WebAPI")
        .WithDownloadButton(true)
        .WithTheme(ScalarTheme.Solarized)
        .WithLayout(ScalarLayout.Classic)
        .WithClientButton(false)
        .WithDarkMode(true)
        .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
}).AllowAnonymous();

// CORS - allow specific origins
app.UseCors(policy =>
    policy.AllowAnyOrigin() // temporarily allowing all origins till we have the final environments ready
        .AllowAnyMethod()
        .AllowAnyHeader());

// Register endpoint groups
app.MapStorageEndpoints()
   .MapSemanticEndpoints();

// Initial vector store setup: upsert all Expense embeddings
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var expenseSemanticRepository = serviceProvider.GetRequiredService<IRepository<ExpenseSemantic>>();
    var vectorStore = serviceProvider.GetRequiredService<SqliteVectorStore>();

    string collectionName = "ExpenseSemantics";
    var expenseSemanticCollection = vectorStore.GetCollection<int, ExpenseSemantic>(collectionName);
    await expenseSemanticCollection.EnsureCollectionExistsAsync().ConfigureAwait(false);

    // Fetch all expenses
    var expenseSemantics = await expenseSemanticRepository.ListAsync();

    foreach (var expenseSemantic in expenseSemantics)
    {
        await expenseSemanticCollection.UpsertAsync(expenseSemantic);
    }
}

app.Run();