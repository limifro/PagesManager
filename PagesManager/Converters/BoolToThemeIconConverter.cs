using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PagesManager.Converters;

public class BoolToThemeIconConverter : IValueConverter
{
    public static readonly BoolToThemeIconConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool isDark && isDark ? "☀️" : "🌙";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}