using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChefKnifeStudios.ExpenseTracker.Shared;

public static class JsonOptions
{
    public static JsonSerializerOptions Get() => new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
