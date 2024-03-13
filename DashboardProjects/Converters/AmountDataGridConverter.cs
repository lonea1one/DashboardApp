using System.Globalization;
using System.Windows.Data;

namespace DashboardProjects.Converters
{
	internal class AmountDataGridConverter : IValueConverter
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
			throw new NotImplementedException();
		}
	}
}
