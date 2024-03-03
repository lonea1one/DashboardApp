using System.Globalization;

namespace DashboardProjects.Utils
{
	public class ChartDataPoint
	{
		public decimal Value { get; }
		public string FormattedValue { get; }
		public string Percentage { get; }

		public ChartDataPoint(decimal value, string percentage)
		{
			var ci = new CultureInfo("ru-RU");
			Value = value;
			FormattedValue = value.ToString("N0", ci);
			Percentage = percentage;
		}
	}
}
