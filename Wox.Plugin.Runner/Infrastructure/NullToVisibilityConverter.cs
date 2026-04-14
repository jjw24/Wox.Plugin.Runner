using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wox.Plugin.Runner.Infrastructure;

internal class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var invert = parameter?.ToString()?.ToLower() == "invert";
        var isNull = value == null || (value is string str && string.IsNullOrWhiteSpace(str));

        if (invert) return isNull ? Visibility.Visible : Visibility.Hidden;
        return isNull ? Visibility.Hidden : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}