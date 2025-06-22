using System.Globalization;

namespace System;

public static class DecimalExtensions
{
    public static string FormatAsDollar(this decimal amount)
    {
        return amount.ToString("C", CultureInfo.GetCultureInfo("en-US"));
    }
}
