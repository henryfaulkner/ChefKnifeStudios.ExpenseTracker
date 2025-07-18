﻿using ChefKnifeStudios.ExpenseTracker.Data.Repos;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace ChefKnifeStudios.ExpenseTracker.Data.Models;

public class ExpenseSemantic : IAggregateRoot
{
    [Key]
    [VectorStoreKey]
    [TextSearchResultName]
    public int Id { get; set; }

    [VectorStoreData]
    public int ExpenseId { get; set; }

    [VectorStoreData]
    [TextSearchResultValue]
    public required string Labels { get; set; } 

    [VectorStoreData]
    public byte[]? SemanticEmbedding { get; set; }

    [VectorStoreData]
    public Guid AppId { get; set; }

    [NotMapped]
    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> SemanticEmbeddingVectors
    {
        get => SemanticEmbedding == null
            ? ReadOnlyMemory<float>.Empty
            : MemoryMarshal.Cast<byte, float>(SemanticEmbedding).ToArray();
    }

    [NotMapped]
    public double Score { get; set; }

    [ForeignKey(nameof(ExpenseId))]
    public Expense? Expense { get; set; }
}
