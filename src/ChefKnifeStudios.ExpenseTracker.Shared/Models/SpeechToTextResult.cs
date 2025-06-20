namespace ChefKnifeStudios.ExpenseTracker.Shared.Models;

public class SpeechToTextResult
{
    public string? Text { get; set; }
    public Exception? Exception { get; set; }
    public bool IsSuccessful { get; set; }
}
