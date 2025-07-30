using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ClosedXML.Excel;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface IExcelViewModel : IViewModel
{
    Task SendEmailWithBudgetExcelAsync(IEnumerable<BudgetDTO> budgets, string emailRecipien);
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
        string fileName = string.Format("budget_{0}.xlsx", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        string filePath = Path.Combine(_fileService.GetCacheFolder(), fileName);

        using var workbook = new XLWorkbook();
        foreach (var budget in budgets)
        {
            var worksheet = workbook.Worksheets.Add(budget.Name);
            // Write headers in row 1
            worksheet.Cell(1, 1).Value = nameof(ExpenseDTO.Name);
            worksheet.Cell(1, 2).Value = nameof(ExpenseDTO.Cost);
            worksheet.Cell(1, 3).Value = nameof(ExpenseDTO.Labels);
            worksheet.Cell(1, 4).Value = nameof(ExpenseDTO.IsRecurring);

            int row = 2; // Start from row 2 for expenses
            foreach (var expense in budget.ExpenseDTOs ?? [])
            {
                worksheet.Cell(row, 1).Value = expense.Name;
                worksheet.Cell(row, 2).Value = expense.Cost;
                worksheet.Cell(row, 3).Value = string.Join(", ", expense.Labels);
                worksheet.Cell(row, 4).Value = expense.IsRecurring;
                row++;
            }
            workbook.SaveAs(filePath);
        }

        await _emailService.SendEmailAsync(
            new EmailItem()
            { 
                Subject = "Your Downloaded Budget",
                Body = "Here is your downloaded budget!",
                Recipients = [emailRecipient],
                FileNames = [fileName],
            }
        );
    }
}
