using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace ChefKnifeStudios.ExpenseTracker.WebAPI.Models;

public class ExpenseVectorStore
{
    [VectorStoreKey]
    public int Id { get; init; }

    [VectorStoreData]
    public required string Name { get; init; }

    [VectorStoreData]
    public required decimal Cost { get; init; }

    [VectorStoreVector(1536)]
    public required Embedding<float> Embedding { get; init; }
}
