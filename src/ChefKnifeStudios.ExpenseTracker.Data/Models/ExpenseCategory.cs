namespace ChefKnifeStudios.ExpenseTracker.Data.Models;

public class ExpenseCategory : BaseEntity
{
    public int ExpenseId { get; set; }
    public int CategoryId { get; set; }

    public Expense? Expense { get; set; }
    public Category? Category { get; set; }
}
