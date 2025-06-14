namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface IToastService
{
    void ShowSuccess(string message, string title = "SUCCESS");
    void ShowWarning(string message, string title = "WARNING");
    void ShowError(string message, string title = "ERROR");
}
