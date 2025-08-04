namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class TextToExpenseResponseDTO
{
    public decimal Price { get; set; }
    public string? Name { get; set; }
    public IEnumerable<string>? Labels { get; set; }
    public IEnumerable<CategoryDTO>? Categories { get; set; }
}
