using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Shell;

namespace LogViewer.Converters
{
    internal class ProgressStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool val)
            {
                return val ? TaskbarItemProgressState.Indeterminate : TaskbarItemProgressState.Normal;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}