using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PagesManager.Converters;

public class BoolToFontStyleConverter : IValueConverter
{
    public static readonly BoolToFontStyleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? FontStyle.Italic : FontStyle.Normal;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}