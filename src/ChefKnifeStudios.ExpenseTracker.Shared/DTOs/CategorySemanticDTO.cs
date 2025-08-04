namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class CategorySemanticDTO
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Labels { get; set; } = string.Empty;
    public ReadOnlyMemory<float> SemanticEmbedding { get; set; } = ReadOnlyMemory<float>.Empty;
}
