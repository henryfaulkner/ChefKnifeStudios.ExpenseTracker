namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class ExpenseSemanticDTO
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    public required string LabelsJson { get; set; }
    public required ReadOnlyMemory<float> SemanticEmbedding { get; set; }
}
