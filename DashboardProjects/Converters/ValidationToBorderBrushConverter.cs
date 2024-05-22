using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DashboardProjects.Converters
{
    public class ValidationToBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isValid = (bool)value;

            return isValid ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#323338")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E58"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
