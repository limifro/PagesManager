using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PagesManager.Converters;

public class StringToTextAlignmentConverter : IValueConverter
{
    public static readonly StringToTextAlignmentConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value as string) switch
        {
            "Center" => TextAlignment.Center,
            "Right"  => TextAlignment.Right,
            _        => TextAlignment.Left
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}