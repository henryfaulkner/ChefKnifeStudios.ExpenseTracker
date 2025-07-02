namespace ChefKnifeStudios.ExpenseTracker.BL.Models;

public class AppSettings
{
    public required AzureDocumentIntelligenceConfig AzureDocumentIntelligence { get; set; }
    public required OpenAIConfig OpenAI { get; set; }
}

public class AzureDocumentIntelligenceConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class OpenAIConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string ChatModelId { get; set; } = string.Empty;
    public string EmbeddingModelId { get; set; } = string.Empty;
}