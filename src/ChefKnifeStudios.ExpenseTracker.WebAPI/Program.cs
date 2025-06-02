using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.Text.Json;
using Azure;
using Azure.AI.DocumentIntelligence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddOpenApi()
    .AddCors();

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

app.MapPost("/scan-receipt", async (HttpRequest request) =>
{
    string endpoint = Environment.GetEnvironmentVariable("FormRecognizer:Endpoint");
    string apiKey = Environment.GetEnvironmentVariable("FormRecognizer:Key");

    if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
    {
        return Results.Problem("Azure Form Recognizer endpoint and key are not configured.");
    }

    var credential = new AzureKeyCredential(apiKey);
    var client = new DocumentIntelligenceClient(new Uri(endpoint), credential);

    // Read the uploaded file
    var formFile = request.Form.Files["file"];
    if (formFile == null)
    {
        return Results.BadRequest("No file was uploaded.");
    }

    using var stream = formFile.OpenReadStream();

    // Analyze the receipt
    AnalyzeDocumentOptions docOptions = new("prebuilt-receipt", BinaryData.FromStream(stream));
    var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, docOptions);

    var receipts = operation.Value;
    var resultData = new List<object>();

    foreach (var receipt in receipts.Documents)
    {
        var receiptData = new Dictionary<string, object>();

        if (receipt.Fields.TryGetValue("MerchantName", out var merchantNameField) && merchantNameField.FieldType == DocumentFieldType.String)
        {
            receiptData["MerchantName"] = merchantNameField.ValueString;
        }

        if (receipt.Fields.TryGetValue("TransactionDate", out var transactionDateField) && transactionDateField.FieldType == DocumentFieldType.Date)
        {
            receiptData["TransactionDate"] = transactionDateField.ValueDate?.ToString("yyyy-MM-dd");
        }

        if (receipt.Fields.TryGetValue("Items", out var itemsField) && itemsField.FieldType == DocumentFieldType.List)
        {
            var items = new List<Dictionary<string, object>>();
            foreach (var itemField in itemsField.ValueList)
            {
                if (itemField.FieldType == DocumentFieldType.Dictionary)
                {
                    var itemData = new Dictionary<string, object>();
                    var itemFields = itemField.ValueDictionary;

                    if (itemFields.TryGetValue("Description", out var descriptionField) && descriptionField.FieldType == DocumentFieldType.String)
                    {
                        itemData["Description"] = descriptionField.ValueString;
                    }

                    if (itemFields.TryGetValue("TotalPrice", out var totalPriceField) && totalPriceField.FieldType == DocumentFieldType.Currency)
                    {
                        itemData["TotalPrice"] = totalPriceField.ValueCurrency.Amount;
                    }

                    items.Add(itemData);
                }
            }
            receiptData["Items"] = items;
        }

        if (receipt.Fields.TryGetValue("Total", out var totalField) && totalField.FieldType == DocumentFieldType.Currency)
        {
            receiptData["Total"] = totalField.ValueCurrency.Amount;
        }

        resultData.Add(receiptData);
    }

    return Results.Ok(resultData);
})
.WithName("ReadReceipt")
.Accepts<IFormFile>("multipart/form-data")
.Produces<List<object>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);

app.Run();