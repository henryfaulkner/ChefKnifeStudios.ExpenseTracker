using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using Microsoft.AspNetCore.Components;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.AreYouSureDialogs;

public partial class DeleteRecurringExpenseAreYouSureDialog : ComponentBase
{
    [Parameter] public required RecurringExpenseConfigDTO RecurringExpense { get; set; }

    
}
