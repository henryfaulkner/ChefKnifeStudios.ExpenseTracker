using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.Shared;

public class Constants
{
    public const string AppIdHeader = "X-App-Id";

    public class DateFormats
    {
        public const string DateOnlyFormat = "MM/dd/yyyy";
        public const string DateTimeFormat = "MM/dd/yyyy @ h:mmtt";
    }
}
