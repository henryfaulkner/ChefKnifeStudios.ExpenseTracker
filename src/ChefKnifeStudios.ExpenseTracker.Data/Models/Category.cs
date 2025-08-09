using ChefKnifeStudios.ExpenseTracker.Data.Enums;

namespace ChefKnifeStudios.ExpenseTracker.Data.Models;

public class Category : BaseEntity
{
    public required string DisplayName { get; set; }
    public CategoryTypes CategoryType { get; set; }
    public required string LabelsJson { get; set; }
    public Guid AppId { get; set; } = Guid.Empty; // Guid.Empty is a wildcard
    public string? Icon { get; set; }

    public CategorySemantic? CategorySemantic { get; set; }
    public ICollection<ExpenseCategory>? ExpenseCategories { get; set; }
}
