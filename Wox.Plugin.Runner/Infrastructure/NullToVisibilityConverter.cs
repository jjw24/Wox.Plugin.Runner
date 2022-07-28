using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Wox.Plugin.Runner.Infrastructure
{
    class NullToVisibilityConverter : IValueConverter
    {
        public object Convert( object? value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return value == null ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
