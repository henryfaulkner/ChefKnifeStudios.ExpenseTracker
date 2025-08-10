using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;

public class BudgetEventArgs : IEventArgs
{
    public enum Types { Added, }

    public required Types Type { get; init; }
}
