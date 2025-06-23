namespace ChefKnifeStudios.ExpenseTracker.Data.Models;
public class RecurringExpenseConfig : BaseEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Cost { get; set; }
    public required string LabelsJson { get; set; }
}
