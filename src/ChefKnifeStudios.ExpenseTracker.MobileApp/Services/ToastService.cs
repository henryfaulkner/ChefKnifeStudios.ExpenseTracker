using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using MatBlazor;
using Microsoft.AspNetCore.Components;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class ToastService(IMatToaster matToaster) : IToastService
{
    readonly IMatToaster _matToaster = matToaster;

    public void ShowSuccess(string message, string title = "SUCCESS")
        => Show(MatToastType.Success, title, message);
    public void ShowWarning(string message, string title = "WARNING")
        => Show(MatToastType.Warning, title, message);
    public void ShowError(string message, string title = "ERROR")
        => Show(MatToastType.Danger, title, message);


    void Show(MatToastType type, string title, string message, string icon = "")
    {
        _matToaster.Add(message, type, title, icon);
    }
}
