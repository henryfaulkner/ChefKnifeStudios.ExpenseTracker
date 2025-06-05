namespace ChefKnifeStudios.ExpenseTracker.Shared.Models;

public class PhotoResult
{
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? ContentType { get; set; }
    public Stream? FileStream { get; set; }
}
