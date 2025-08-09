namespace ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

public class AddExpenseRequestDTO
{
    public required ExpenseDTO Expense { get; set; }
    public required ReceiptLabelsDTO ReceiptLabels { get; set; }
}
