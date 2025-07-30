using ChefKnifeStudios.ExpenseTracker.Shared.Models;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface IEmailService
{
    Task SendEmailAsync(EmailItem item);
}
