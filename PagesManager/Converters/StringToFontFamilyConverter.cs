using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PagesManager.Converters;

public class StringToFontFamilyConverter : IValueConverter
{
    public static readonly StringToFontFamilyConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string name || string.IsNullOrWhiteSpace(name))
            return FontFamily.Default;

        try
        {
            return new FontFamily(name);
        }
        catch
        {
            return FontFamily.Default;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FontFamily ff) return ff.Name;
        return value?.ToString();
    }
}