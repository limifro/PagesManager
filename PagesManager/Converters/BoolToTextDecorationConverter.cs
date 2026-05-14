using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PagesManager.Converters;

public class BoolToTextDecorationConverter : IValueConverter
{
    public static readonly BoolToTextDecorationConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? TextDecorations.Underline : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}