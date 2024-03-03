using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;

namespace DashboardProjects.Controls
{
	public sealed partial class CustomToolTip : IChartTooltip
	{
		private TooltipData _data;

		public CustomToolTip()
		{
			InitializeComponent();
			DataContext = this;
		}

		public TooltipData Data
		{
			get => _data;
			set
			{
				_data = value;
				OnPropertyChanged(nameof(Data));
			}
		}

		public TooltipSelectionMode? SelectionMode { get; set; }

		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged(string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
