using ChefKnifeStudios.ExpenseTracker.Data;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.WebAPI;
using ChefKnifeStudios.ExpenseTracker.WebAPI.EndpointGroups;
using ChefKnifeStudios.ExpenseTracker.BL.Models;
using ChefKnifeStudios.ExpenseTracker.BL.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Npgsql;
using OpenAI;
using Scalar.AspNetCore;
using ChefKnifeStudios.ExpenseTracker.BL;

var builder = WebApplication.CreateBuilder(args);

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
if (appSettings == null) throw new ApplicationException("AppSettings are not configured correctly.");

string connectionString = builder.Configuration.GetConnectionString("ExpenseTrackerDB")!;

builder.Services
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddCors()
    .AddHttpClient()
    .AddTransient<IHttpService, HttpService>()
    .RegisterDataServices(connectionString)
    .AddScoped<IStorageService, StorageService>()
    .AddScoped<ISemanticService, SemanticService>()
    .AddPostgresVectorStore(_ => connectionString); //https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/postgres-connector?pivots=programming-language-csharp

builder.Services
    .AddKernel()
    .ConfigureSemanticKernel(appSettings);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

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
    var vectorStore = serviceProvider.GetRequiredService<PostgresVectorStore>();

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