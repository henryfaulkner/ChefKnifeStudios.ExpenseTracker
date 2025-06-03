using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.Text.Json;
using Azure;
using Azure.AI.DocumentIntelligence;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using System.Globalization;
using ChefKnifeStudios.ExpenseTracker.WebAPI;
using ChefKnifeStudios.ExpenseTracker.WebAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddCors()
    .AddHttpClient()
    .AddTransient<IHttpService, HttpService>();

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
    var resultData = new List<ReceiptResponse>();

    foreach (var receipt in receipts.Documents)
    {
        var receiptResponse = new ReceiptResponse();

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
.Produces<List<ReceiptResponse>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);

app.MapPost("/label-receipt-json", async (HttpRequest request, IConfiguration config, IHttpService httpService) =>
{
    var section = config.GetSection("OpenAI");
    string? endpoint = section.GetValue<string>("Endpoint") ?? string.Empty;
    string? apiKey = section.GetValue<string>("Key") ?? string.Empty;

    httpService.SetBearerAuthentication(apiKey);


    string prompt = "Prewritten prompt";
    var chatGptRequest = new ChatGPTRequest
    {
        Model = "gpt-3.5-turbo",
        Messages = new List<ChatGPTMessage>
                {
                    new ChatGPTMessage { Role = "system", Content = "You are now an advanced machine learning model specifically designed to analyze receipt data. Your sole function is to process receipt details provided in JSON format and generate relevant labels that describe the nature of the receipt. These labels will enhance search functionality by categorizing the receipt effectively.\r\n\r\nGuidelines:\r\nYour output must always be a JSON object containing a single string array property named \"labels\".\r\n\r\nEach label should be concise, descriptive, and accurately represent the receipt's content or purpose.\r\n\r\nPrioritize specificity and clarity to ensure the labels are useful for search and categorization.\r\n\r\nExample:\r\nInput (JSON):\r\n\r\njson\r\nCopy\r\nEdit\r\n{\r\n  \"date\": \"2025-06-01\",\r\n  \"merchant\": \"Starbucks\",\r\n  \"items\": [\"Caffe Latte\", \"Blueberry Muffin\"],\r\n  \"total\": 7.50,\r\n  \"payment_method\": \"Credit Card\"\r\n}\r\nOutput (JSON):\r\n\r\njson\r\nCopy\r\nEdit\r\n{\r\n  \"labels\": [\"Coffee Shop\", \"Breakfast\", \"Food & Beverage\"]\r\n}\r\nPlease proceed with processing the receipt data accordingly." },
                    new ChatGPTMessage { Role = "user", Content = $"Analyze the following receipt data and generate labels:\n{prompt}" }
                },
        Temperature = 0.0f
    };

    var res = await httpService.PostAsync<ChatGPTRequest, ChatGPTResponse?>(endpoint, chatGptRequest);

    string message = res.Data?.Choices.FirstOrDefault()?.Message.Content ?? "fuck the api failed.";
    return Results.Ok(message);
})
.WithName("LabelReceiptJson")
.Accepts<ReceiptResponse>("application/json")
.Produces<List<object>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);

app.Run();