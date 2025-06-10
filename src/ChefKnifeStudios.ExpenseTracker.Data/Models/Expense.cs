using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

public class Expense : BaseEntity
{
    [Key]
    [VectorStoreKey]
    [TextSearchResultName]
    public override int Id { get; set; }

    public int BudgetId { get; set; }

    [VectorStoreData]
    [TextSearchResultValue]
    public required string Name { get; set; }

    public decimal Cost { get; set; }

    [VectorStoreData]
    public required string LabelsJson { get; set; }

    public byte[]? SemanticEmbedding { get; set; }

    [NotMapped]
    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> SemanticEmbeddingVectors
    {
        get => SemanticEmbedding == null
            ? ReadOnlyMemory<float>.Empty
            : MemoryMarshal.Cast<byte, float>(SemanticEmbedding).ToArray();
        set => SemanticEmbedding = MemoryMarshal.AsBytes(value.Span).ToArray();
    }
}
