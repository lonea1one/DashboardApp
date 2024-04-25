using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DashboardProjects.Converters
{
	internal class DateDataGridConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateTime dateTime)
			{
				return dateTime.ToString("dd.MM.yyyy");
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not string dateString) return value;
			if (DateTime.TryParseExact(dateString, "dd.MM.yyyy", culture, DateTimeStyles.None, out var dateTime))
			{
				if (dateTime >= new DateTime(2023, 1, 1))
				{
					return dateTime;
				}

				MessageBox.Show("Дата должна быть не ранее 2023 года", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			else
			{
				MessageBox.Show("Некорректная дата", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			return value;
		}
	}
}
