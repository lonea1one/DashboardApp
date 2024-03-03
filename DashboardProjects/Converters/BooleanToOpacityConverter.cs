using System.Globalization;
using System.Windows.Data;

namespace DashboardProjects.Converters
{
	public class BooleanToOpacityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? 1.0 : .5; // Полная видимость или полупрозрачность
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
