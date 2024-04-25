using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DashboardProjects.Converters
{
	public class AmountDataGridConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is decimal amount)
			{
				return $"{amount:F2} ₽";
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not string stringValue) return value;
			if (decimal.TryParse(stringValue.Replace(" ₽", ""), out decimal amount))
			{
				return amount;
			}
			
			MessageBox.Show("Некорректная сумма", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
			return value;

		}
	}
}
