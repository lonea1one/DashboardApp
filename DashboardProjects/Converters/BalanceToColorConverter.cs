using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DashboardProjects.Converters
{
	public class BalanceToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string stringValue && decimal.TryParse(stringValue, out var number))
			{
				if (number == 0)
					return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9"));

				return number >= 0 ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EF06D")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E58"));
			}

			return new SolidColorBrush(Colors.Black); // Возвращаем стандартный цвет, если преобразование невозможно
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException(); // Обратное преобразование не требуется
		}
	}
}
