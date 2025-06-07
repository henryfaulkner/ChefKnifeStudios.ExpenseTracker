using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class ReceiptLabelsDTO
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("labels")]
    public IEnumerable<string>? Labels { get; set; }
}
