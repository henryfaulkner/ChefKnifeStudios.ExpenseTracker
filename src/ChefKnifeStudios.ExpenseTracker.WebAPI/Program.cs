using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.Text.Json;
using Azure;
using Azure.AI.DocumentIntelligence;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using System.Globalization;
using ChefKnifeStudios.ExpenseTracker.WebAPI;
using ChefKnifeStudios.ExpenseTracker.WebAPI.Models;
using OpenAI;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddCors()
    .AddHttpClient()
    .AddTransient<IHttpService, HttpService>();

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

app.MapPost("/scan-receipt", async (HttpRequest request, IConfiguration config) =>
{
    var section = config.GetSection("FormRecognizer");
    string? endpoint = section.GetValue<string>("Endpoint");
    string? apiKey = section.GetValue<string>("Key");

    if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
    {
        return Results.Problem("Azure Form Recognizer endpoint and key are not configured.");
    }

    var credential = new AzureKeyCredential(apiKey);
    var client = new DocumentIntelligenceClient(new Uri(endpoint), credential);

    // Read the uploaded file
    if (!request.Form.Files.Any())
    {
        return Results.BadRequest("No file was uploaded.");
    }
    var formFile = request.Form.Files[0];
    if (formFile == null)
    {
        return Results.BadRequest("File upload failed.");
    }

    using var stream = formFile.OpenReadStream();

    // Analyze the receipt
    AnalyzeDocumentOptions docOptions = new("prebuilt-receipt", BinaryData.FromStream(stream));
    var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, docOptions);

    var receipts = operation.Value;
    var resultData = new List<ReceiptDTO>();

    foreach (var receipt in receipts.Documents)
    {
        var receiptResponse = new ReceiptDTO();

        if (receipt.Fields.TryGetValue("MerchantName", out var merchantNameField) && merchantNameField.FieldType == DocumentFieldType.String)
        {
            receiptResponse.MerchantName = merchantNameField.ValueString;
        }

        if (receipt.Fields.TryGetValue("TransactionDate", out var transactionDateField) && transactionDateField.FieldType == DocumentFieldType.Date)
        {
            string? transactionDateString = transactionDateField.ValueDate?.ToString("yyyy-MM-dd");

            if (!string.IsNullOrEmpty(transactionDateString))
            {
                receiptResponse.TransactionDate = DateTime.ParseExact(transactionDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            else
            {
                Console.WriteLine("Transaction date string is null or empty.");
            }
        }

        if (receipt.Fields.TryGetValue("Items", out var itemsField) && itemsField.FieldType == DocumentFieldType.List)
        {
            foreach (var itemField in itemsField.ValueList)
            {
                if (itemField.FieldType == DocumentFieldType.Dictionary)
                {
                    var item = new Item();
                    var itemFields = itemField.ValueDictionary;

                    if (itemFields.TryGetValue("Description", out var descriptionField) && descriptionField.FieldType == DocumentFieldType.String)
                    {
                        item.Description = descriptionField.ValueString;
                    }

                    if (itemFields.TryGetValue("TotalPrice", out var totalPriceField) && totalPriceField.FieldType == DocumentFieldType.Currency)
                    {
                        item.TotalPrice = (decimal)totalPriceField.ValueCurrency.Amount;
                    }

                    if (receiptResponse.Items == null) receiptResponse.Items = new List<Item>();
                    receiptResponse.Items.Add(item);
                }
            }
        }

        if (receipt.Fields.TryGetValue("Total", out var totalField) && totalField.FieldType == DocumentFieldType.Currency)
        {
            receiptResponse.Total = (decimal)totalField.ValueCurrency.Amount;
        }

        resultData.Add(receiptResponse);
    }

    return Results.Ok(resultData);
})
.WithName("ReadReceipt")
.Accepts<IFormFile>("multipart/form-data")
.Produces<List<ReceiptDTO>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);

app.MapPost("/label-receipt-details", async (HttpRequest request, IConfiguration config, OpenAIClient openAiClient) =>
{
    // Read the receipt JSON input from the request body
    string prompt;
    using (var reader = new StreamReader(request.Body))
    {
        prompt = await reader.ReadToEndAsync();
    }

    // Define the messages for the chat completion
    var messages = new List<ChatMessage>
    {
        ChatMessage.CreateSystemMessage(
            @"You are an advanced machine learning model specifically designed to analyze receipt data. Your sole function is to process receipt details provided in JSON format and generate a relevant name and labels that describe the nature of the receipt. These labels will enhance search functionality by categorizing the receipt effectively.
            Guidelines:
            Your output must always be a JSON object containing:
            - A string property named `name` summarizing the transaction.
            - A string array property named `labels` describing the receipt's content or purpose.

            Example:
            Input (JSON):
            {
              ""date"": ""2025-06-01"",
              ""merchant"": ""Starbucks"",
              ""items"": [""Caffe Latte"", ""Blueberry Muffin""],
              ""total"": 7.50,
              ""payment_method"": ""Credit Card""
            }
            Output (JSON):
            {
             ""name"": ""Starbucks breakfast"",
             ""labels"": [""Coffee Shop"", ""Breakfast"", ""Food & Beverage""]
            }
            Please process the provided receipt data accordingly."
        ),
        ChatMessage.CreateUserMessage($"Analyze the following receipt data and generate labels:\n{prompt}")
    };

    try
    {
        // Call the OpenAI API using the SDK
        var chatClient = openAiClient.GetChatClient("gpt-3.5-turbo");
        var completionResponse = await chatClient.CompleteChatAsync(messages);

        var responseContent = completionResponse.Value.Content.FirstOrDefault()?.Text ?? "The model failed to generate response text";

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }

        // Deserialize the response into ReceiptLabelsDTO
        var labels = JsonSerializer.Deserialize<ReceiptLabelsDTO>(responseContent)
            ?? new ReceiptLabelsDTO { Labels = Array.Empty<string>() };

        return Results.Ok(labels);
    }
    catch (Exception ex)
    {
        // Log exception (if necessary) and return error
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("LabelReceiptJson")
.Accepts<ReceiptDTO>("application/json")
.Produces<ReceiptLabelsDTO>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);

app.Run();