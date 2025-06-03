namespace ChefKnifeStudios.ExpenseTracker.WebAPI.Models;

public class ApiException
{
    // Description of exception
    public string? Message { get; set; }

    // Numeric code assigned to specific exception
    public int Code { get; set; }

    // Immediate frames on the call stack
    public string? StackTrace { get; set; }

    // System.Exception instance that caused the current exception
    public string? InnerExcpetion { get; set; }
}