using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using System.Text;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface IExcelViewModel : IViewModel
{
    Task SendEmailWithBudgetExcelAsync(IEnumerable<BudgetDTO> budgets, string emailRecipient);
}

public class ExcelViewModel : BaseViewModel, IExcelViewModel
{
    readonly IEmailService _emailService;
    readonly IFileService _fileService;

    public ExcelViewModel(IEmailService emailService, IFileService fileService)
    {
        _emailService = emailService;
        _fileService = fileService;
    }

    public async Task SendEmailWithBudgetExcelAsync(IEnumerable<BudgetDTO> budgets, string emailRecipient)
    {
        var fileNames = new List<string>();

        foreach (var budget in budgets)
        {
            string safeBudgetName = string.Join("_", budget.Name.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"budget_{safeBudgetName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
            string filePath = Path.Combine(_fileService.GetCacheFolder(), fileName);

            var csv = new StringBuilder();
            // Write headers
            csv.AppendLine("Name,Cost,Labels,Categories,IsRecurring");

            foreach (var expense in budget.ExpenseDTOs ?? Enumerable.Empty<ExpenseDTO>())
            {
                string labels = string.Join(";", expense.Labels ?? Enumerable.Empty<string>());
                string categories = string.Join(";", expense.Categories?.Select(c => c.ToString()) ?? Enumerable.Empty<string>());
                string line = $"\"{expense.Name}\",{expense.Cost},\"{labels}\",\"{categories}\",{expense.IsRecurring}";
                csv.AppendLine(line);
            }

            File.WriteAllText(filePath, csv.ToString());
            fileNames.Add(fileName);
        }

        await _emailService.SendEmailAsync(
            new EmailItem()
            {
                Subject = "Your Downloaded Budget",
                Body = "Here is your downloaded budget!",
                Recipients = [emailRecipient],
                FileNames = fileNames,
            }
        );
    }
}
