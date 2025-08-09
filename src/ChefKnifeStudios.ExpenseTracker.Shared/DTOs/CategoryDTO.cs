using ChefKnifeStudios.ExpenseTracker.Shared.Enums;

namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class CategoryDTO
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public CategoryTypes CategoryType { get; set; }
    public IEnumerable<string> Labels { get; set; } = [];
    public Guid AppId { get; set; }
    public string? Icon { get; set; }
}