using System.Globalization;
using System.Windows.Data;

namespace DashboardProjects.Converters
{
	public class CurrencyFormatConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string stringValue && decimal.TryParse(stringValue, out var number))
			{
				return number == 0 ? "-" : $"{number:N0} ₽";
			}

			return value; // Возвращаем исходное значение, если преобразование невозможно
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException(); // Обратное преобразование не требуется
		}
	}
}
