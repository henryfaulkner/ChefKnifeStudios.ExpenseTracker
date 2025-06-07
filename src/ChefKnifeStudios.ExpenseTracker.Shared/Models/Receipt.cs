using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using System.Diagnostics.CodeAnalysis;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Models;

public class Receipt
{
    [SetsRequiredMembers]
    public Receipt()
    {
        MerchantName = string.Empty;
        TransactionDate = DateTime.Now;
        Items = new List<Item>();
        Total = 0.0m;
        Name = string.Empty;
        Labels = new List<string>();
    }

    [SetsRequiredMembers]
    public Receipt(ReceiptDTO receipt, ReceiptLabelsDTO labels)
    {
        MerchantName = receipt.MerchantName ?? string.Empty;
        TransactionDate = receipt.TransactionDate ?? DateTime.Now;
        Items = receipt.Items ?? new List<Item>();
        Total = receipt.Total ?? 0.0m;
        Name = labels.Name ?? string.Empty;
        Labels = labels.Labels?.ToList() ?? [];
    }

    public string MerchantName { get; set; }
    public DateTime TransactionDate { get; set; }
    public required List<Item> Items { get; set; }
    public decimal Total { get; set; }
    public required string Name { get; set; }
    public required List<string> Labels { get; set; }

    public override string ToString()
    {
        var itemsDescription = string.Join(", ", Items.Select(item => $"{item.Description} (${item.TotalPrice:F2})"));
        var labelsDescription = string.Join(", ", Labels);

        return $@"
            Receipt:
            ---------
            Merchant: {MerchantName}
            Date: {TransactionDate:yyyy-MM-dd HH:mm:ss}
            Items: [{itemsDescription}]
            Total: ${Total:F2}
            Name: {Name}
            Labels: [{labelsDescription}]
        ";
    }
}
