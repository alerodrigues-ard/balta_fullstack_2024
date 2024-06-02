using System;

namespace Fina.Core.Common;

public static class DateTimeExtension
{
    // Extension methods for DateTime

    // Primeiro dia do mês
    public static DateTime GetFirstDay(this DateTime date, int? year = null, int? month = null) 
        => new(year ?? date.Year, month ?? date.Month, 1);

    // Último dia do mês
    public static DateTime GetLastDay(this DateTime date, int? year = null, int? month = null) 
        => new DateTime(year ?? date.Year, month ?? date.Month, 1)
            .AddMonths(1)
            .AddDays(-1);
}
