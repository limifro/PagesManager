using System;
using System.Globalization;

namespace PagesManager.Core.Helpers;

public static class DateFormatter
{
    private static readonly CultureInfo Ru = new("ru-RU");
    public static string FormatRelative(DateTime dateUtc, DateTime nowUtc)
    {
        var date = dateUtc.ToLocalTime();
        var now = nowUtc.ToLocalTime();

        var today = now.Date;
        var dateDay = date.Date;

        if (dateDay == today)
            return date.ToString("HH:mm", Ru);

        if (dateDay == today.AddDays(-1))
            return "Вчера";

        var daysDiff = (today - dateDay).Days;
        if (daysDiff > 1 && daysDiff < 7)
        {
            var dayName = Ru.DateTimeFormat.GetDayName(date.DayOfWeek);
            return char.ToUpper(dayName[0]) + dayName.Substring(1);
        }

        if (date.Year == now.Year)
            return date.ToString("d MMMM", Ru);

        return date.ToString("dd.MM.yyyy", Ru);
    }
}