using System.Text.Json.Serialization;

namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class ReceiptLabelsDTO
{
    public string? Name { get; set; }
    public DateTime? CreatedOn { get; set; } 
    public IEnumerable<string>? Labels { get; set; }
}
