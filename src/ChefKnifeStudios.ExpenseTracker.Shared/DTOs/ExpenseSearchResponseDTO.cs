namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class ExpenseSearchResponseDTO
{
    public required string RagMessage { get; set; } 
    public required IEnumerable<ExpenseDTO> Expenses { get; set; }
}
