using ChefKnifeStudios.ExpenseTracker.Data.Models;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    [VectorStoreData]
    public decimal Cost { get; set; }

    [VectorStoreData]
    public required string LabelsJson { get; set; }

    [VectorStoreVector(1536)] // Use the correct dimension for your embedding model
    public required ReadOnlyMemory<float> SemanticEmbedding { get; set; }

    [ForeignKey(nameof(BudgetId))]
    public Budget? Budget { get; set; }
}
