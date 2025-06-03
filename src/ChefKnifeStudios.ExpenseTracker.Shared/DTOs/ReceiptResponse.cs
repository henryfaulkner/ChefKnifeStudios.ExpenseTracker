namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class ReceiptResponse
{
    public string? MerchantName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public List<Item>? Items { get; set; }
    public decimal? Total { get; set; }
}

public class Item
{
    public required string Description { get; set; }
    public decimal? TotalPrice { get; set; }
}

