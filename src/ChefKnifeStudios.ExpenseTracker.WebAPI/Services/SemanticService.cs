using Azure;
using Azure.AI.DocumentIntelligence;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.Data.Specifications;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.WebAPI.Models;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using System.Globalization;
using System.Text.Json;

namespace ChefKnifeStudios.ExpenseTracker.WebAPI.Services;

public interface ISemanticService
{
    Task<List<ReceiptDTO>> ScanReceiptAsync(Stream fileStream);
    Task<TextToExpenseResponseDTO> TextToExpenseAsync(string prompt);
    Task<ReceiptLabelsDTO> LabelReceiptDetailsAsync(string receiptJson);
    Task<SemanticEmbeddingDTO> CreateSemanticEmbeddingAsync(ReceiptLabelsDTO receiptLabels);
    Task<List<ExpenseSearchResponseDTO>> SearchExpensesAsync(ExpenseSearchDTO searchRequest);
}

public class SemanticService : ISemanticService
{
    private readonly ILogger<SemanticService> _logger;
    private readonly IConfiguration _config;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IRepository<Expense> _expenseRepository;
    private readonly SqliteVectorStore _vectorStore;

    public SemanticService(
        ILogger<SemanticService> logger,
        IConfiguration config,
        IChatCompletionService chatCompletionService,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IRepository<Expense> expenseRepository,
        SqliteVectorStore vectorStore)
    {
        _logger = logger;
        _config = config;
        _chatCompletionService = chatCompletionService;
        _embeddingGenerator = embeddingGenerator;
        _expenseRepository = expenseRepository;
        _vectorStore = vectorStore;
    }

    public async Task<List<ReceiptDTO>> ScanReceiptAsync(Stream fileStream)
    {
        var appSettings = _config.GetSection("AppSettings").Get<AppSettings>();
        if (appSettings == null) throw new ApplicationException("AppSettings are not configured correctly.");

        if (string.IsNullOrEmpty(appSettings.AzureDocumentIntelligence.Endpoint) ||
            string.IsNullOrEmpty(appSettings.AzureDocumentIntelligence.ApiKey))
        {
            throw new ApplicationException("Azure Form Recognizer endpoint and key are not configured.");
        }

        var credential = new AzureKeyCredential(appSettings.AzureDocumentIntelligence.ApiKey);
        var client = new DocumentIntelligenceClient(
            new Uri(appSettings.AzureDocumentIntelligence.Endpoint), credential);

        // Analyze the receipt
        AnalyzeDocumentOptions docOptions = new("prebuilt-receipt", BinaryData.FromStream(fileStream));
        var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, docOptions);

        var receipts = operation.Value;
        var resultData = new List<ReceiptDTO>();

        foreach (var receipt in receipts.Documents)
        {
            var receiptResponse = new ReceiptDTO();

            if (receipt.Fields.TryGetValue("MerchantName", out var merchantNameField) &&
                merchantNameField.FieldType == DocumentFieldType.String)
            {
                receiptResponse.MerchantName = merchantNameField.ValueString;
            }

            if (receipt.Fields.TryGetValue("TransactionDate", out var transactionDateField) &&
                transactionDateField.FieldType == DocumentFieldType.Date)
            {
                string? transactionDateString = transactionDateField.ValueDate?.ToString("yyyy-MM-dd");

                if (!string.IsNullOrEmpty(transactionDateString))
                {
                    receiptResponse.TransactionDate = DateTime.ParseExact(transactionDateString,
                        "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
            }

            if (receipt.Fields.TryGetValue("Items", out var itemsField) &&
                itemsField.FieldType == DocumentFieldType.List)
            {
                foreach (var itemField in itemsField.ValueList)
                {
                    if (itemField.FieldType == DocumentFieldType.Dictionary)
                    {
                        var item = new Item();
                        var itemFields = itemField.ValueDictionary;

                        if (itemFields.TryGetValue("Description", out var descriptionField) &&
                            descriptionField.FieldType == DocumentFieldType.String)
                        {
                            item.Description = descriptionField.ValueString;
                        }

                        if (itemFields.TryGetValue("TotalPrice", out var totalPriceField) &&
                            totalPriceField.FieldType == DocumentFieldType.Currency)
                        {
                            item.TotalPrice = (decimal)totalPriceField.ValueCurrency.Amount;
                        }

                        if (receiptResponse.Items == null) receiptResponse.Items = new List<Item>();
                        receiptResponse.Items.Add(item);
                    }
                }
            }

            if (receipt.Fields.TryGetValue("Total", out var totalField) &&
                totalField.FieldType == DocumentFieldType.Currency)
            {
                receiptResponse.Total = (decimal)totalField.ValueCurrency.Amount;
            }

            resultData.Add(receiptResponse);
        }

        return resultData;
    }

    public async Task<TextToExpenseResponseDTO> TextToExpenseAsync(string prompt)
    {
        ChatHistory chatHistory = new();
        chatHistory.AddSystemMessage(
            @"
                You are an advanced machine learning model specifically designed to analyze text, extract a price, and summarize the text into a name and descriptive labels. Your sole function is to process text details provided in JSON format and extract a price and generate a relevant name and labels that describe the nature of the text. The text will most likely be describing a merchant expense. The labels will enhance search functionality by categorizing the text effectively as a merchant expense
.               Guidelines:
                Your output must always be a JSON object containing:
                - A decimal property named `price` reflecting the transactions total cost. 
                - A string property named `name` summarizing the transaction.
                - A string array property named `labels` describing the receipt's content or purpose.

                Example:
                Input (JSON):
                {
                    ""text"": ""I spent five dollars a the grocery store.""
                }
                Output (JSON):
                {
                    ""price"": 5.00
                    ""name"": ""Grocery Shopping"",
                    ""labels"": [""Food"", ""Groceries"", ""Food & Beverage"", ""Shopping"", ""Essentials""]
                }
                Please process the provided expense data accordingly.
            "
        );
        chatHistory.AddUserMessage($"Analyze the following text and generate an output according to the provided guideline document:\n{prompt}");

        // Call chat service
        var message = await _chatCompletionService.GetChatMessageContentAsync(chatHistory);

        var responseContent = message.Content ?? "The model failed to generate response text";

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            throw new ApplicationException("AI model returned empty response");
        }

        // Deserialize the response into TextToExpenseResponseDTO
        var result = JsonSerializer.Deserialize<TextToExpenseResponseDTO>(responseContent, Shared.JsonOptions.Get())
            ?? new TextToExpenseResponseDTO { Name = string.Empty, Labels = Array.Empty<string>() };

        return result;
    }

    public async Task<ReceiptLabelsDTO> LabelReceiptDetailsAsync(string receiptJson)
    {
        // Define the messages for the chat completion
        ChatHistory chatHistory = new();
        chatHistory.AddSystemMessage(
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
        );
        chatHistory.AddUserMessage($"Analyze the following receipt data and generate labels:\n{receiptJson}");

        // Call chat service
        var message = await _chatCompletionService.GetChatMessageContentAsync(chatHistory);

        var responseContent = message.Content ?? "The model failed to generate response text";

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            throw new ApplicationException("AI model returned empty response");
        }

        // Deserialize the response into ReceiptLabelsDTO
        var result = JsonSerializer.Deserialize<ReceiptLabelsDTO>(responseContent, Shared.JsonOptions.Get())
            ?? new ReceiptLabelsDTO { Labels = Array.Empty<string>() };

        return result;
    }

    public async Task<SemanticEmbeddingDTO> CreateSemanticEmbeddingAsync(ReceiptLabelsDTO receiptLabels)
    {
        List<string> list = receiptLabels?.Labels?.ToList() ?? new List<string>();
        if (!string.IsNullOrWhiteSpace(receiptLabels?.Name)) list.Add(receiptLabels.Name);

        var text = string.Join(" ", list);
        var embedding = await _embeddingGenerator.GenerateAsync(text);

        SemanticEmbeddingDTO result = new()
        {
            Labels = text,
            Embedding = embedding.Vector,
        };

        return result;
    }

    public async Task<List<ExpenseSearchResponseDTO>> SearchExpensesAsync(ExpenseSearchDTO searchRequest)
    {
        if (searchRequest is null) throw new ArgumentNullException(nameof(searchRequest));

        Embedding<float> queryEmbedding = await _embeddingGenerator.GenerateAsync(searchRequest.SearchText);

        // Get and create collection if it doesn't exist.
        var collectionName = "ExpenseSemantics";
        var expenseSemanticCollection = _vectorStore.GetCollection<int, ExpenseSemantic>(collectionName);
        await expenseSemanticCollection.EnsureCollectionExistsAsync().ConfigureAwait(false);

        List<ExpenseSemantic> semanticResult = [];
        // uses distance
        // Lower score = more similar
        // Higher score = less similar
        var searchResult = expenseSemanticCollection.SearchAsync(queryEmbedding, top: searchRequest.TopN);
        await foreach (var expenseVectorResult in searchResult)
        {
            if (!expenseVectorResult.Score.HasValue) continue;
            var record = expenseVectorResult.Record;
            record.Score = expenseVectorResult.Score.Value;
            semanticResult.Add(record);
        }

        var expenseIds = semanticResult
            .OrderBy(x => x.Score)
            .Select(x => x.ExpenseId)
            .ToList();
        var expenses = await _expenseRepository.ListAsync(
            new GetExpensesByIdsSpec(expenseIds)
        );
        // Must return in the same order as expenseIds
        // Could be optimize to sort out of memory
        var orderedExpenses = expenseIds
            .Select(id => expenses.FirstOrDefault(e => e.Id == id))
            .Where(e => e != null) // Exclude nulls in case some IDs are not found
            .ToList();

        List<ExpenseSearchResponseDTO> result = [];
        foreach (var expense in orderedExpenses)
        {
            result.Add(
                new()
                {
                    ExpenseId = expense.Id,
                    ExpenseName = expense.Name,
                    Cost = expense.Cost,
                    BudgetName = expense.Budget?.Name ?? string.Empty,
                }
            );
        }

        return result;
    }
}
