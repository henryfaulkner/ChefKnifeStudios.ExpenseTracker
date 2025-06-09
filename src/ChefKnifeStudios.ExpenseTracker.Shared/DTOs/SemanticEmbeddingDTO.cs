namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class SemanticEmbeddingDTO
{
    public required ReadOnlyMemory<float> Embedding { get; set; }
}
