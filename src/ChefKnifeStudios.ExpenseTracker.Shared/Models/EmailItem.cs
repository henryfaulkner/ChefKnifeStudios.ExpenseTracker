namespace ChefKnifeStudios.ExpenseTracker.Shared.Models;

public record EmailItem
{
    public string? Subject;
    public string? Body;
    public required IEnumerable<string> Recipients;
    public IEnumerable<string>? FileNames;
}
