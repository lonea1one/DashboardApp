using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DashboardProjects.Converters;

public class TypeDataGridConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string stringValue) return value;
        if (stringValue.Equals("РАСХОДЫ", StringComparison.OrdinalIgnoreCase))
        {
            return "РАСХОДЫ";
        }

        if (stringValue.Equals("ДОХОДЫ", StringComparison.OrdinalIgnoreCase))
        {
            return "ДОХОДЫ";
        }
        
        MessageBox.Show("Некорректный тип транзакции", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        return value;
    }
}