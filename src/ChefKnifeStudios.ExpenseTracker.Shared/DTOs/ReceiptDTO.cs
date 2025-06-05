using System.Text.Json.Serialization;

namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class ReceiptDTO
{
    public ReceiptDTO()
    {
        Items = new List<Item>();
    }

    [JsonPropertyName("merchantName")]
    public string? MerchantName { get; set; }
    [JsonPropertyName("transactionDate")]
    public DateTime? TransactionDate { get; set; }
    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }
    [JsonPropertyName("total")]
    public decimal? Total { get; set; }
}

public class Item
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("totalPrice")]
    public decimal? TotalPrice { get; set; }
}

