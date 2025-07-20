using Azure;
using Azure.AI.DocumentIntelligence;
using ChefKnifeStudios.ExpenseTracker.BL.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Models;
using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using ChefKnifeStudios.ExpenseTracker.Data.Specifications;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.PgVector;
using System.Globalization;
using System.Text.Json;

namespace ChefKnifeStudios.ExpenseTracker.BL.Services;

public interface ISemanticService
{
    Task<List<ReceiptDTO>> ScanReceiptAsync(Stream fileStream, Guid appId, CancellationToken cancellationToken = default);
    Task<TextToExpenseResponseDTO> TextToExpenseAsync(string prompt, Guid appId, CancellationToken cancellationToken = default);
    Task<ReceiptLabelsDTO> LabelReceiptDetailsAsync(string receiptJson, Guid appId, CancellationToken cancellationToken = default);
    Task<SemanticEmbeddingDTO> CreateSemanticEmbeddingAsync(ReceiptLabelsDTO receiptLabels, Guid appId, CancellationToken cancellationToken = default);
    Task<ExpenseSearchResponseDTO> SearchExpensesAsync(ExpenseSearchDTO searchRequest, Guid appId, CancellationToken cancellationToken = default);
}

public class SemanticService : ISemanticService
{
    private readonly ILogger<SemanticService> _logger;
    private readonly IConfiguration _config;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IRepository<Expense> _expenseRepository;
    private readonly PostgresVectorStore _vectorStore;

    public SemanticService(
        ILogger<SemanticService> logger,
        IConfiguration config,
        IChatCompletionService chatCompletionService,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IRepository<Expense> expenseRepository,
        PostgresVectorStore vectorStore)
    {
        _logger = logger;
        _config = config;
        _chatCompletionService = chatCompletionService;
        _embeddingGenerator = embeddingGenerator;
        _expenseRepository = expenseRepository;
        _vectorStore = vectorStore;
    }

    public async Task<List<ReceiptDTO>> ScanReceiptAsync(Stream fileStream, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ScanReceiptAsync started. AppId: {AppId}", appId);
        try
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

            AnalyzeDocumentOptions docOptions = new("prebuilt-receipt", BinaryData.FromStream(fileStream));
            var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, docOptions, cancellationToken);

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

            _logger.LogInformation("ScanReceiptAsync completed. AppId: {AppId}, Receipts: {Count}", appId, resultData.Count);
            return resultData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ScanReceiptAsync failed. AppId: {AppId}", appId);
            throw;
        }
    }

    public async Task<TextToExpenseResponseDTO> TextToExpenseAsync(string prompt, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("TextToExpenseAsync started. AppId: {AppId}, Prompt: {Prompt}", appId, prompt);
        try
        {
            ChatHistory chatHistory = new();
            chatHistory.AddSystemMessage(
                @"
                    You are an advanced machine learning model specifically designed to analyze text, extract a price, and summarize the text into a name and descriptive labels. Your sole function is to process text details provided in JSON format and extract a price and generate a relevant name and labels that describe the nature of the text. The text will most likely be describing a merchant expense. The labels will enhance search functionality by categorizing the text effectively as a merchant expense
.                   Guidelines:
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

            var message = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

            var responseContent = message.Content ?? "The model failed to generate response text";

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                throw new ApplicationException("AI model returned empty response");
            }

            var result = JsonSerializer.Deserialize<TextToExpenseResponseDTO>(responseContent, Shared.JsonOptions.Get())
                ?? new TextToExpenseResponseDTO { Name = string.Empty, Labels = Array.Empty<string>() };

            _logger.LogInformation("TextToExpenseAsync completed. AppId: {AppId}, Name: {Name}, Price: {Price}", appId, result.Name, result.Price);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TextToExpenseAsync failed. AppId: {AppId}, Prompt: {Prompt}", appId, prompt);
            throw;
        }
    }

    public async Task<ReceiptLabelsDTO> LabelReceiptDetailsAsync(string receiptJson, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("LabelReceiptDetailsAsync started. AppId: {AppId}, ReceiptJson: {ReceiptJson}", appId, receiptJson);
        try
        {
            ChatHistory chatHistory = new();
            chatHistory.AddSystemMessage(
                @"You are an advanced machine learning model specifically designed to analyze receipt data. Your sole function is to process receipt details provided in JSON format and generate a relevant name and labels that describe the nature of the receipt. These labels will enhance search functionality by categorizing the receipt effectively.
                Guidelines:
                Your output must always be a JSON object containing:
                - A string property named `name` summarizing the transaction.
                - A string array property named `labels` describing the receipt's content or purpose, the date the transaction occurred, and any ancillary details able to be divined in order to support search.

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

            var message = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

            var responseContent = message.Content ?? "The model failed to generate response text";

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                throw new ApplicationException("AI model returned empty response");
            }

            var result = JsonSerializer.Deserialize<ReceiptLabelsDTO>(responseContent, Shared.JsonOptions.Get())
                ?? new ReceiptLabelsDTO { Labels = Array.Empty<string>() };

            _logger.LogInformation("LabelReceiptDetailsAsync completed. AppId: {AppId}, Name: {Name}", appId, result.Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LabelReceiptDetailsAsync failed. AppId: {AppId}, ReceiptJson: {ReceiptJson}", appId, receiptJson);
            throw;
        }
    }

    public async Task<SemanticEmbeddingDTO> CreateSemanticEmbeddingAsync(ReceiptLabelsDTO receiptLabels, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CreateSemanticEmbeddingAsync started. AppId: {AppId}, Labels: {Labels}, Name: {Name}", appId, receiptLabels?.Labels, receiptLabels?.Name);
        try
        {
            List<string> list = receiptLabels?.Labels?.ToList() ?? new List<string>();
            if (!string.IsNullOrWhiteSpace(receiptLabels?.Name)) list.Add(receiptLabels.Name);

            var text = string.Join(" ", list);
            var embedding = await _embeddingGenerator.GenerateAsync(text, cancellationToken: cancellationToken);

            SemanticEmbeddingDTO result = new()
            {
                Labels = text,
                Embedding = embedding.Vector,
            };

            _logger.LogInformation("CreateSemanticEmbeddingAsync completed. AppId: {AppId}, Labels: {Labels}", appId, result.Labels);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateSemanticEmbeddingAsync failed. AppId: {AppId}, Labels: {Labels}, Name: {Name}", appId, receiptLabels?.Labels, receiptLabels?.Name);
            throw;
        }
    }

    public async Task<ExpenseSearchResponseDTO> SearchExpensesAsync(ExpenseSearchDTO searchRequest, Guid appId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SearchExpensesAsync started. AppId: {AppId}, SearchText: {SearchText}", appId, searchRequest?.SearchText);
        try
        {
            if (searchRequest is null) throw new ArgumentNullException(nameof(searchRequest));

            Embedding<float> queryEmbedding = await _embeddingGenerator.GenerateAsync(searchRequest.SearchText, cancellationToken: cancellationToken);

            var collectionName = "ExpenseSemantics";
            var expenseSemanticCollection = _vectorStore.GetCollection<int, ExpenseSemantic>(collectionName);
            await expenseSemanticCollection.EnsureCollectionExistsAsync().ConfigureAwait(false);

            List<ExpenseSemantic> semanticResult = [];
            var searchResult = expenseSemanticCollection.SearchAsync(queryEmbedding, top: searchRequest.TopN, cancellationToken: cancellationToken);
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
                new GetExpensesByIdsSpec(expenseIds, appId),
                cancellationToken
            );
            var orderedExpenses = expenseIds
                .Select(id => expenses.FirstOrDefault(e => e.Id == id))
                .Where(e => e != null)
                .ToList();

            List<ExpenseDTO> expenseList = [];
            foreach (var expense in orderedExpenses)
            {
                if (expense == null) continue;
                expenseList.Add(expense.MapToDTO());
            }
            var ragMessage = (await CreateRagMessageAsync(expenseList, cancellationToken)) ?? string.Empty;

            var result = new ExpenseSearchResponseDTO() { RagMessage = ragMessage, Expenses =  expenseList, };

            _logger.LogInformation("SearchExpensesAsync completed. AppId: {AppId}, Results: {Count}", appId, expenseList.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SearchExpensesAsync failed. AppId: {AppId}, SearchText: {SearchText}", appId, searchRequest?.SearchText);
            throw;
        }
    }

    private async Task<string?> CreateRagMessageAsync(IEnumerable<ExpenseDTO> expenses, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CreateRagMessageAsync started.");
        try
        {
            // Serialize only relevant fields for summarization
            var simplifiedExpenses = expenses.Select(e => new
            {
                e.Name,
                e.Cost,
                e.Labels,
                e.IsRecurring,
                BudgetName = e.Budget?.Name
            });

            var expensesJson = JsonSerializer.Serialize(simplifiedExpenses, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            // System message: instruct the AI model
            var systemMessage = @"
                You are an advanced AI assistant. Your task is to analyze a list of expense records provided in JSON format.
                Summarize the data in natural language, highlighting patterns, categories, frequent labels, total and average costs, and any notable recurring expenses.
                If possible, mention the most common budgets and any interesting insights.
                Do not repeat the raw data; instead, provide a concise, human-readable summary.";

            // User message: provide the serialized data
            var userMessage = $"Here is the expense data:\n{expensesJson}";

            ChatHistory chatHistory = new();
            chatHistory.AddSystemMessage(systemMessage);
            chatHistory.AddUserMessage(userMessage);

            var message = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

            var responseContent = message.Content ?? string.Empty;

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                throw new ApplicationException("AI model returned empty response");
            }

            var result = responseContent;

            _logger.LogInformation("CreateRagMessageAsync completed. Result: {0}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateRagMessageAsync failed.");
            throw;
        }
    }
}