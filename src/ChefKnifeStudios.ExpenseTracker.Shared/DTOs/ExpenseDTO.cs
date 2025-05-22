namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class ExpenseDTO 
{
    public int Id { get; set; } = 0;
    public decimal Cost { get; set; }
    public required string Name { get; set; }
}
