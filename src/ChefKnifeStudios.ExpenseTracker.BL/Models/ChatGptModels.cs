using System.Text.Json.Serialization;

namespace ChefKnifeStudios.ExpenseTracker.BL.Models;

public class ChatGPTRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("messages")]
    public List<ChatGPTMessage> Messages { get; set; } = new();

    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; } = 0.7f;

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; } = null;

    [JsonPropertyName("top_p")]
    public float? TopP { get; set; } = null;

    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; } = null;

    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; } = null;
}

public class ChatGPTMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } // "system", "user", or "assistant"

    [JsonPropertyName("content")]
    public string Content { get; set; }
}

public class ChatGPTResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; }

    [JsonPropertyName("created")]
    public int Created { get; set; }

    [JsonPropertyName("choices")]
    public List<ChatGPTChoice> Choices { get; set; }

    [JsonPropertyName("usage")]
    public ChatGPTUsage Usage { get; set; }
}

public class ChatGPTChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public ChatGPTMessage Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
}

public class ChatGPTUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
