using ChefKnifeStudios.ExpenseTracker.Shared.Models;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

// MS Learn for MAUI Email
//https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/communication/email?view=net-maui-9.0&tabs=macios
public class EmailService : IEmailService
{
    readonly IFileService _fileService;

    public EmailService(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async Task SendEmailAsync(EmailItem item)
    {
        if (Email.Default.IsComposeSupported)
        {
            var message = new EmailMessage
            {
                Subject = item.Subject,
                Body = item.Body,
                BodyFormat = EmailBodyFormat.PlainText,
                To = item.Recipients.ToList(),
                Attachments = item.FileNames?
                    .Select(x => new EmailAttachment(Path.Combine(_fileService.GetCacheFolder(), x)))
                    .ToList()
            };
            await Email.Default.ComposeAsync(message);
        }
    }
}
