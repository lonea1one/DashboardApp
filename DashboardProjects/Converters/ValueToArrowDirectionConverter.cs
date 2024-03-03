﻿using System.Globalization;
using System.Windows.Data;

namespace DashboardProjects.Converters
{
	public class ValueToArrowDirectionConverter : IValueConverter
	{
		private const string ArrowDown = "M169.4 470.6c12.5 12.5 32.8 12.5 45.3 0l160-160c12.5-12.5 12.5-32.8 0-45.3s-32.8-12.5-45.3 0L224 370.8 224 64c0-17.7-14.3-32-32-32s-32 14.3-32 32l0 306.7L54.6 265.4c-12.5-12.5-32.8-12.5-45.3 0s12.5 32.8 0 45.3l160 160z";
		private const string ArrowUp = "M214.6 41.4c-12.5-12.5-32.8-12.5-45.3 0l-160 160c-12.5 12.5-12.5 32.8 0 45.3s32.8 12.5 45.3 0L160 141.2V448c0 17.7 14.3 32 32 32s32-14.3 32-32V141.2L329.4 246.6c12.5 12.5 32.8 12.5 45.3 0s12.5-32.8 0-45.3l-160-160z";
		private const string ArrowDefault = "";

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string stringValue && decimal.TryParse(stringValue, out var number))
			{
				if (number == 0)
					return ArrowDefault;

				return number >= 0 ? ArrowUp : ArrowDown;
			}

			return value; // Возвращаем стрелку вверх по умолчанию или ArrowDown в зависимости от вашего предпочтения
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}