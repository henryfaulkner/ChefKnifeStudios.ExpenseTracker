using System.Text.Json.Serialization;

namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class SemanticEmbeddingDTO
{
    [JsonPropertyName("embedding")]
    public required ReadOnlyMemory<float> Embedding { get; set; }
}
