using System.Text.Json.Serialization;

namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class SemanticEmbeddingDTO
{
    public required string Labels { get; set; }
    [JsonPropertyName("embedding")]
    public required ReadOnlyMemory<float> Embedding { get; set; }
}
