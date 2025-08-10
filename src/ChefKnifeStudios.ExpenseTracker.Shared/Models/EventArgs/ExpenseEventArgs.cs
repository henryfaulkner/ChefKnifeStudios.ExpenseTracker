using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;

public class ExpenseEventArgs : IEventArgs
{
    public enum Types { Added, Deleted, }

    public required Types Type { get; init; }
}
