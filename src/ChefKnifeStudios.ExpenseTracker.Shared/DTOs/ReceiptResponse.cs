namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class ReceiptResponse
{
    public ReceiptResponse()
    {
        Items = new List<Item>();
    }

    public string? MerchantName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public List<Item>? Items { get; set; }
    public decimal? Total { get; set; }
}

public class Item
{
    public string? Description { get; set; }
    public decimal? TotalPrice { get; set; }
}

