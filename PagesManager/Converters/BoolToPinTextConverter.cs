using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PagesManager.Converters;

public class BoolToPinTextConverter : IValueConverter
{
    public static readonly BoolToPinTextConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isPinned)
            return isPinned ? "Открепить" : "Закрепить";
        return "Закрепить";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}