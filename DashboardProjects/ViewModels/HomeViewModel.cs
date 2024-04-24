using DashboardProjects.Commands;
using DashboardProjects.Utils;
using DataAccess;
using DataAccess.Models;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;


namespace DashboardProjects.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private ObservableCollection<string>? _expenseCategories;
	public ObservableCollection<string>? ExpenseCategories
	{
		get => _expenseCategories;
		set
		{
			_expenseCategories = value;
			OnPropertyChanged(nameof(ExpenseCategories));
		}
	}

	private ObservableCollection<string>? _incomeCategories;
	public ObservableCollection<string>? IncomeCategories
	{
		get => _incomeCategories;
		set
		{
			_incomeCategories = value;
			OnPropertyChanged(nameof(IncomeCategories));
		}
	}

	private ObservableCollection<string>? _years;
	public ObservableCollection<string>? Years
	{
		get => _years;
		set
		{
			_years = value;
			OnPropertyChanged(nameof(Years));
		}
	}

	private ObservableCollection<int>? _selectedExpenses;
	public ObservableCollection<int>? SelectedExpenses
	{
		get => _selectedExpenses;
		set
		{
			_selectedExpenses = value;
			OnPropertyChanged(nameof(SelectedExpenses));
		}
	}

	private ObservableCollection<int>? _selectedIncomes;
	public ObservableCollection<int>? SelectedIncomes
	{
		get => _selectedIncomes;
		set
		{
			_selectedIncomes = value;
			OnPropertyChanged(nameof(SelectedIncomes));
		}
	}

	private ObservableCollection<int>? _selectedYears;
	public ObservableCollection<int>? SelectedYears
	{
		get => _selectedYears;
		set
		{
			_selectedYears = value;
			OnPropertyChanged(nameof(SelectedYears));
		}
	}

	public SeriesCollection ExpenseCollection { get; }
	public SeriesCollection IncomeCollection { get; }

	public SeriesCollection ChartExpenseCollection { get; }
	public SeriesCollection ChartIncomeCollection { get; }

	public SeriesCollection BalanceChartSeries { get; }

	public Func<double, string> ExpenseYFormatter { get; private set; }
	public Func<double, string> IncomeYFormatter { get; private set; }
	public Func<double, string> BalanceYFormatter { get; private set; }

	public ICommand PieChartExpenseDataClickCommand { get; private set; }
	public ICommand PieChartExpenseDataHoverCommand { get; private set; }
	public ICommand PieChartExpenseMouseLeaveCommand { get; private set; }

	public ICommand ExpenseChartDataHoverCommand { get; private set; }
	public ICommand ExpenseChartMouseLeaveCommand { get; private set; }
	public ICommand IncomeChartDataHoverCommand { get; private set; }
	public ICommand IncomeChartMouseLeaveCommand { get; private set; }

	public ICommand LbMouseLeftButtonDownCommand { get; private set; }
	public ICommand LbMouseMoveCommand { get; private set; }
	public ICommand LbMouseLeftButtonUpCommand { get; private set; }

	public ICommand FileDragOverCommand { get; private set; }
	public ICommand FileDropCommand { get; private set; }

	private string? _selectedExpenseSum;
	public string? SelectedExpenseSum
	{
		get => _selectedExpenseSum;
		set
		{
			_selectedExpenseSum = value;
			OnPropertyChanged(nameof(SelectedExpenseSum));
		}
	}

	private string? _selectedIncomeSum;
	public string? SelectedIncomeSum
	{
		get => _selectedIncomeSum;
		set
		{
			_selectedIncomeSum = value;
			OnPropertyChanged(nameof(SelectedIncomeSum));
		}
	}

	private string? _totalExpenseSum;
	public string? TotalExpenseSum
	{
		get => _totalExpenseSum;
		set
		{
			_totalExpenseSum = value;
			OnPropertyChanged(nameof(TotalExpenseSum));
		}
	}

	private string? _totalBalance;
	public string? TotalBalance
	{
		get => _totalBalance;
		set
		{
			_totalBalance = value;
			OnPropertyChanged(nameof(TotalBalance));
		}
	}

	private string? _totalIncomeSum;
	public string? TotalIncomeSum
	{
		get => _totalIncomeSum;
		set
		{
			_totalIncomeSum = value;
			OnPropertyChanged(nameof(TotalIncomeSum));
		}
	}

	private string? _percentageExpense;
	public string? PercentageExpense
	{
		get => _percentageExpense;
		set
		{
			_percentageExpense = value;
			OnPropertyChanged(nameof(PercentageExpense));
		}
	}

	private string? _percentageIncome;
	public string? PercentageIncome
	{
		get => _percentageIncome;
		set
		{
			_percentageIncome = value;
			OnPropertyChanged(nameof(PercentageIncome));
		}
	}

	private string? _firstQuartalBalance;
	public string? FirstQuartalBalance
	{
		get => _firstQuartalBalance;
		set
		{
			_firstQuartalBalance = value;
			OnPropertyChanged(nameof(FirstQuartalBalance));
		}
	}

	private string? _secondQuartalBalance;
	public string? SecondQuartalBalance
	{
		get => _secondQuartalBalance;
		set
		{
			_secondQuartalBalance = value;
			OnPropertyChanged(nameof(SecondQuartalBalance));
		}
	}

	private string? _thirdQuartalBalance;
	public string? ThirdQuartalBalance
	{
		get => _thirdQuartalBalance;
		set
		{
			_thirdQuartalBalance = value;
			OnPropertyChanged(nameof(ThirdQuartalBalance));
		}
	}

	private string? _fourdQuartalBalance;
	public string? FourdQuartalBalance
	{
		get => _fourdQuartalBalance;
		set
		{
			_fourdQuartalBalance = value;
			OnPropertyChanged(nameof(FourdQuartalBalance));
		}
	}

	private string[]? _expenseLabels;
	public string[]? ExpenseLabels
	{
		get => _expenseLabels;
		set
		{
			_expenseLabels = value;
			OnPropertyChanged(nameof(ExpenseLabels));
		}
	}

	private string[]? _incomeLabels;
	public string[]? IncomeLabels
	{
		get => _incomeLabels;
		set
		{
			_incomeLabels = value;
			OnPropertyChanged(nameof(IncomeLabels));
		}
	}

	private string[]? _balanceLabels;
	public string[]? BalanceLabels
	{
		get => _balanceLabels;
		set
		{
			_balanceLabels = value;
			OnPropertyChanged(nameof(BalanceLabels));
		}
	}

	private string _selectedExpenseAmount;
	public string SelectedExpenseAmount
	{
		get => _selectedExpenseAmount;
		set
		{
			_selectedExpenseAmount = value;
			OnPropertyChanged(nameof(SelectedExpenseAmount));
		}
	}

	private string _selectedIncomeAmount;
	public string SelectedIncomeAmount
	{
		get => _selectedIncomeAmount;
		set
		{
			_selectedIncomeAmount = value;
			OnPropertyChanged(nameof(SelectedIncomeAmount));
		}
	}

	private string _selectedPercentageExpense;
	public string SelectedPercentageExpense
	{
		get => _selectedPercentageExpense;
		set
		{
			_selectedPercentageExpense = value;
			OnPropertyChanged(nameof(SelectedPercentageExpense));
		}
	}

	private string _selectedPercentageIncome;
	public string SelectedPercentageIncome
	{
		get => _selectedPercentageIncome;
		set
		{
			_selectedPercentageIncome = value;
			OnPropertyChanged(nameof(SelectedPercentageIncome));
		}
	}

	private Point _startPoint;
	public Point StartPoint
	{
		get => _startPoint;
		set
		{
			_startPoint = value;
			OnPropertyChanged(nameof(StartPoint));
		}
	}

	private bool _isSelecting;
	public bool IsSelecting
	{
		get => _isSelecting;
		set
		{
			_isSelecting = value;
			OnPropertyChanged(nameof(IsSelecting));
		}
	}

	private bool _hasExpenseData;
	public bool HasExpenseData
	{
		get => _hasExpenseData;
		set
		{
			if (_hasExpenseData == value) return;
			_hasExpenseData = value;
			OnPropertyChanged(nameof(HasExpenseData));
		}
	}

	private bool _hasIncomeData;
	public bool HasIncomeData
	{
		get => _hasIncomeData;
		set
		{
			if (_hasIncomeData == value) return;
			_hasIncomeData = value;
			OnPropertyChanged(nameof(HasIncomeData));
		}
	}

	private bool _isChartIncomeAvailable;
	public bool IsChartIncomeAvailable
	{
		get => _isChartIncomeAvailable;
		set
		{
			_isChartIncomeAvailable = value;
			OnPropertyChanged(nameof(IsChartIncomeAvailable));
		}
	}

	private bool _isChartExpenseAvailable;
	public bool IsChartExpenseAvailable
	{
		get => _isChartExpenseAvailable;
		set
		{
			_isChartExpenseAvailable = value;
			OnPropertyChanged(nameof(IsChartExpenseAvailable));
		}
	}

	private Visibility _progressVisibility = Visibility.Collapsed;
	public Visibility ProgressVisibility
	{
		get => _progressVisibility;
		set
		{
			if (_progressVisibility == value) return;
			_progressVisibility = value;
			OnPropertyChanged(nameof(ProgressVisibility));
		}
	}

	private Visibility _uploadVisibility = Visibility.Visible;
	public Visibility UploadVisibility
	{
		get => _uploadVisibility;
		set
		{
			if (_uploadVisibility == value) return;
			_uploadVisibility = value;
			OnPropertyChanged(nameof(UploadVisibility));
		}
	}

	private bool _mouseMoved;
	public bool MouseMoved
	{
		get => _mouseMoved;
		set
		{
			_mouseMoved = value;
			OnPropertyChanged(nameof(MouseMoved));
		}
	}

	public HomeViewModel()
	{
		ExpenseCollection = new SeriesCollection();
		IncomeCollection = new SeriesCollection();

		ChartExpenseCollection = new SeriesCollection();
		ChartIncomeCollection = new SeriesCollection();

		BalanceChartSeries = new SeriesCollection();

		ExpenseCategories = [];
		IncomeCategories = [];
		Years = [];

		SelectedExpenses = [];
		SelectedIncomes = [];
		SelectedYears = [];

		PieChartExpenseDataClickCommand = new RelayCommand(OnPieChartDataClick);
		PieChartExpenseDataHoverCommand = new RelayCommand(OnPieChartDataHover);
		PieChartExpenseMouseLeaveCommand = new RelayCommand(OnPieChartMouseLeave);

		ExpenseChartDataHoverCommand = new RelayCommand(OnExpenseChartDataHover);
		ExpenseChartMouseLeaveCommand = new RelayCommand<MouseEventArgs>(OnExpenseChartMouseLeave);
		IncomeChartDataHoverCommand = new RelayCommand(OnIncomeChartDataHover);
		IncomeChartMouseLeaveCommand = new RelayCommand<MouseEventArgs>(OnIncomeChartMouseLeave);

		LbMouseLeftButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(OnPreviewMouseLeftButtonDown);
		LbMouseMoveCommand = new RelayCommand<MouseEventArgs>(OnPreviewMouseMove);
		LbMouseLeftButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(OnPreviewMouseLeftButtonUp);

		FileDragOverCommand = new RelayCommand<DragEventArgs>(OnDragOver);
		FileDropCommand = new RelayCommand<DragEventArgs>(OnFileDrop);

		MapChart();
		_ = LoadDataAsync();
	}

	#region Обработчики событий
	private void OnPreviewMouseLeftButtonDown(object parameter)
	{
		var args = parameter as MouseButtonEventArgs;
		if (args?.Source is not ListBox listBox) return;

		var isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
		var isAltPressed = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;

		if (isCtrlPressed || isAltPressed) return;
		StartPoint = args.GetPosition(listBox);
		IsSelecting = true;
	}

	private void OnPreviewMouseMove(object parameter)
	{
		var args = parameter as MouseEventArgs;
		if (args?.Source is not ListBox listBox || !IsSelecting) return;

		var isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
		var isAltPressed = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;

		if (isCtrlPressed || isAltPressed) return;
		
		MouseMoved = true;
		
		var currentPoint = args.GetPosition(listBox);
		var rect = new Rect(StartPoint, currentPoint);
		UpdateSelection(listBox, rect);
	}

	private void OnPreviewMouseLeftButtonUp(object parameter)
	{
		var args = parameter as MouseButtonEventArgs;
		if (args?.Source is not ListBox listBox) return;

		var isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
		var isAltPressed = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;

		if (isCtrlPressed || isAltPressed) return;

		IsSelecting = false;
		if (!MouseMoved)
		{
			UpdateSelection(listBox, new Rect(), true);
		}
		MouseMoved = false;

		switch (listBox.Name)
		{
			case "ExpenseLb":
				UpdatePieChartAsync("РАСХОДЫ", ExpenseCategories.Where((category, index) => SelectedExpenses.Contains(index)).ToList(), ExpenseCollection);
				break;
			case "IncomeLb":
				UpdatePieChartAsync("ДОХОДЫ", IncomeCategories.Where((category, index) => SelectedIncomes.Contains(index)).ToList(), IncomeCollection);
				break;
			case "YearLb":
				// Если необходимо выполнить общее действие для всех других случаев
				UpdatePieChartAsync("РАСХОДЫ", ExpenseCategories.Where((category, index) => SelectedExpenses.Contains(index)).ToList(), ExpenseCollection);
				UpdatePieChartAsync("ДОХОДЫ", IncomeCategories.Where((category, index) => SelectedIncomes.Contains(index)).ToList(), IncomeCollection);
				break;
		}
	}

	private void OnPieChartDataClick(object parameter)
	{
		if (parameter is not ChartPoint chartPoint) return;
		// Определение, к какому типу относится кликнутая категория
		var isExpense = ExpenseCollection.Any(series => series.Title == chartPoint.SeriesView.Title);
		SelectCategory(chartPoint.SeriesView.Title, isExpense);
	}

	private static void OnPieChartDataHover(object parameter)
	{
		Mouse.OverrideCursor = Cursors.Hand;
	}

	private static void OnPieChartMouseLeave(object parameter)
	{
		Mouse.OverrideCursor = Cursors.Arrow;
	}

	private void OnExpenseChartDataHover(object parameter)
	{
		if (parameter is not ChartPoint chartPoint) return;
		SelectedExpenseAmount = $"Расходы: {chartPoint.Y:N0}₽";

		if (chartPoint.Instance is ChartDataPoint dataPoint)
		{
			SelectedPercentageExpense = $"Процент: {dataPoint.Percentage}";
		}
	}

	private void OnExpenseChartMouseLeave(object parameter)
	{
		SelectedExpenseAmount = string.Empty;
		SelectedPercentageExpense = string.Empty;
	}

	private void OnIncomeChartDataHover(object parameter)
	{
		if (parameter is not ChartPoint chartPoint) return;
		SelectedIncomeAmount = $"Расходы: {chartPoint.Y:N0}₽";

		if (chartPoint.Instance is ChartDataPoint dataPoint)
		{
			SelectedPercentageIncome = $"Процент: {dataPoint.Percentage}";
		}
	}

	private void OnIncomeChartMouseLeave(object parameter)
	{
		SelectedIncomeAmount = string.Empty;
		SelectedPercentageIncome = string.Empty;
	}

	private static void OnDragOver(object parameter)
	{
		var e = parameter as DragEventArgs;

		e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

		e.Handled = true;
	}

	private async void OnFileDrop(object parameter)
	{
		if (parameter is not DragEventArgs args) return;

		if (!args.Data.GetDataPresent(DataFormats.FileDrop)) return;
		var files = (string[])args.Data.GetData(DataFormats.FileDrop);
		if (files.Length <= 0 ||
		    !Path.GetExtension(files[0]).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)) return;
		UploadVisibility = Visibility.Collapsed;
		ProgressVisibility = Visibility.Visible; // Показываем ProgressBar

		try
		{
			await using var context = new DashboardDbContext();

			// Передаем путь к файлу и ожидаем завершения асинхронной операции
			await context.ReadAndSaveExcelData(files[0]);

			Application.Current.Dispatcher.Invoke(InitializeSelectedItems);
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Произошла ошибка при обработке файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		finally
		{
			// Скрываем ProgressBar и возвращаем иконку и надписи
			ProgressVisibility = Visibility.Collapsed;
			UploadVisibility = Visibility.Visible;
		}
	}
	#endregion

	#region Асинхронная загрузка данных
	private async Task LoadDataAsync()
	{
		await using var context = new DashboardDbContext();

		var expenses = await context.Transactions.Where(x => x.Type == "РАСХОДЫ").Select(m => m.Category).Distinct().ToListAsync();
		var incomes = await context.Transactions.Where(x => x.Type == "ДОХОДЫ").Select(m => m.Category).Distinct().ToListAsync();
		var years = await context.Transactions.Select(m => m.Date.Year.ToString()).Distinct().ToListAsync();

		// Обновление UI происходит в потоке UI
		Application.Current.Dispatcher.Invoke(() => UpdateUi(expenses, incomes, years));
	}

	private void UpdateUi(List<string> expenses, List<string> incomes, List<string> years)
	{
		ExpenseCategories = new ObservableCollection<string>(expenses);
		IncomeCategories = new ObservableCollection<string>(incomes);
		Years = new ObservableCollection<string>(years);
		InitializeSelectedItems();
	}
	#endregion

	#region Расчет кварталов
	private async void CalculateQuarterlyBalancesAsync()
	{
		await using var context = new DashboardDbContext();

		// Преобразование выбранных индексов годов в список годов
		var selectedYearValues = SelectedYears.Select(index => Years[index]).ToList();

		// Фильтрация транзакций по выбранным годам
		var transactions = await context.Transactions
			.Where(t => selectedYearValues.Contains(t.Date.Year.ToString()))
			.ToListAsync();

		var quarterlySums = transactions
			.GroupBy(t => new { t.Date.Year, Quarter = ((t.Date.Month - 1) / 3) + 1 })
			.Select(g => new
			{
				Year = g.Key.Year,
				Quarter = g.Key.Quarter,
				TotalIncome = g.Where(t => t.Type == "ДОХОДЫ").Sum(t => (double)t.Amount),
				TotalExpense = g.Where(t => t.Type == "РАСХОДЫ").Sum(t => (double)t.Amount)
			})
			.ToList();

		// Сброс значений кварталов перед обновлением
		FirstQuartalBalance = SecondQuartalBalance = ThirdQuartalBalance = FourdQuartalBalance = "0";

		// Обновление свойств ViewModel для каждого выбранного года
		foreach (var summary in quarterlySums)
		{
			var balance = summary.TotalIncome - summary.TotalExpense;
			var balanceFormatted = balance.ToString();

			if (selectedYearValues.Contains(summary.Year.ToString()))
			{
				switch (summary.Quarter)
				{
					case 1:
						FirstQuartalBalance = UpdateQuartalBalance(FirstQuartalBalance, balanceFormatted);
						break;
					case 2:
						SecondQuartalBalance = UpdateQuartalBalance(SecondQuartalBalance, balanceFormatted);
						break;
					case 3:
						ThirdQuartalBalance = UpdateQuartalBalance(ThirdQuartalBalance, balanceFormatted);
						break;
					case 4:
						FourdQuartalBalance = UpdateQuartalBalance(FourdQuartalBalance, balanceFormatted);
						break;
				}
			}
		}
	}

	private static string UpdateQuartalBalance(string currentBalance, string newBalance)
	{
		// Преобразование текущего баланса в число для суммирования
		var current = decimal.Parse(currentBalance);
		var update = decimal.Parse(newBalance);
		return (current + update).ToString("N0");
	}
	#endregion

	#region Инициализация диаграммы и графика
	private void InitializeSelectedItems()
	{
		SelectedExpenses?.Clear();
		SelectedIncomes?.Clear();
		SelectedYears?.Clear();

		for (var i = 0; i < ExpenseCategories?.Count; i++) SelectedExpenses?.Add(i);
		for (var i = 0; i < IncomeCategories?.Count; i++) SelectedIncomes?.Add(i);
		for (var i = 0; i < Years?.Count; i++) SelectedYears?.Add(i);

		UpdateChartData(ChartExpenseCollection, "РАСХОДЫ");
		UpdateChartData(ChartIncomeCollection, "ДОХОДЫ");

		UpdateBalanceChartData();
		UpdateBalanceForCurrentMonth();
		CalculateQuarterlyBalancesAsync();

		InitializePieChartSeries("РАСХОДЫ", ExpenseCollection, GetExpenseChartColors());
		InitializePieChartSeries("ДОХОДЫ", IncomeCollection, GetIncomeChartColors());
	}

	private async void InitializePieChartSeries(string type, SeriesCollection collection, List<Brush> colors)
	{
		var data = await GetCategoryAsync(type);
		var totalSum = data.Sum(item => item.Sum);

		HasExpenseData = true;
		HasIncomeData = true;

		Application.Current.Dispatcher.Invoke(() =>
		{
			collection.Clear();
			foreach (var (category, sum) in data)
			{
				var brush = colors[collection.Count % colors.Count].Clone();
				brush.Opacity = 0.9;

				collection.Add(new PieSeries
				{
					Title = category,
					Values = new ChartValues<decimal> { sum },
					DataLabels = false,
					Fill = brush,
					StrokeThickness = 0
				});
			}

			switch (type)
			{
				case "РАСХОДЫ":
					UpdatePieChartLabels(totalSum, true);
					UpdateChartData(ChartExpenseCollection, "РАСХОДЫ");
					break;
				case "ДОХОДЫ":
					UpdatePieChartLabels(totalSum, false);
					UpdateChartData(ChartIncomeCollection, "ДОХОДЫ");
					break;
			}
		});
	}

	private static async Task<List<(string Category, decimal Sum)>> GetCategoryAsync(string type)
	{
		await using var context = new DashboardDbContext();
		var categories = await context.Transactions
			.Where(x => x.Type == type)
			.GroupBy(t => t.Category)
			.Select(g => new { Category = g.Key, Sum = g.Sum(t => (double)t.Amount) })
			.ToListAsync();

		return categories.Select(item => (item.Category, (decimal)item.Sum)).ToList();
	}

	private static List<Brush> GetExpenseChartColors()
	{
		return
		[
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EF06D")),
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34EAE6")),
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7030A0")),
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAAD7E")),
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#01A4F6")),
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E58"))
		];
	}

	private static List<Brush> GetIncomeChartColors()
	{
		return
		[
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#01A4F6")),
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E58")),
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34EAE6")),
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EF06D")),
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7030A0")),
			new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAAD7E"))
		];
	}

	private void UpdatePieChartLabels(decimal totalSum, bool isExpense)
	{
		var ci = new CultureInfo("ru-RU");
		if (isExpense)
		{
			SelectedExpenseSum = totalSum.ToString("N0", ci);
			TotalExpenseSum = totalSum.ToString("N2", ci) + " ₽";
			PercentageExpense = "100%";
		}
		else
		{
			SelectedIncomeSum = totalSum.ToString("N0", ci);
			TotalIncomeSum = totalSum.ToString("N2", ci) + " ₽";
			PercentageIncome = "100%";
		}
	}

	private void UpdateChartLabelsForNoData(string transactionType)
	{
		const string noDataText = "";
		if (transactionType == "РАСХОДЫ")
		{
			SelectedExpenseSum = noDataText;
			PercentageExpense = noDataText;
		}
		else
		{
			SelectedIncomeSum = noDataText;
			PercentageIncome = noDataText;
		}
	}
	#endregion

	#region Обновление графика
	private async Task<List<(DateTime Month, decimal TotalAmount, decimal Percentage)>> FetchChartDataAsync(string type)
	{
		await using var context = new DashboardDbContext();

		var selectedCategories = type == "РАСХОДЫ"
			? ExpenseCategories.Where((category, index) => SelectedExpenses.Contains(index)).ToList()
			: IncomeCategories.Where((category, index) => SelectedIncomes.Contains(index)).ToList();

		var selectedYears = Years.Where((year, index) => SelectedYears.Contains(index)).ToList();

		var transactions = await context.Transactions
			.Where(t => selectedYears.Contains(t.Date.Year.ToString()) && selectedCategories.Contains(t.Category) && t.Type == type)
			.ToListAsync();

		var transactionsByMonth = transactions
			.GroupBy(t => new { Year = t.Date.Year, Month = t.Date.Month })
			.Select(group =>
			{
				var totalAmount = (decimal)group.Sum(t => (double)t.Amount);
				var yearTotal = (decimal)transactions.Where(t => t.Date.Year == group.Key.Year).Sum(t => (double)t.Amount);

				return new
				{
					YearMonth = new DateTime(group.Key.Year, group.Key.Month, 1),
					TotalAmount = totalAmount,
					PercentageOfTheYear = totalAmount / yearTotal * 100
				};
			})
			.OrderBy(x => x.YearMonth)
			.ToList();

		return transactionsByMonth.Select(x => (x.YearMonth, x.TotalAmount, x.PercentageOfTheYear)).ToList();
	}

	private async void UpdateChartData(SeriesCollection targetCollection, string transactionType)
	{
		var chartData = await FetchChartDataAsync(transactionType);

		Application.Current.Dispatcher.Invoke(() =>
		{
			if (chartData.Count == 0 || chartData.All(x => x.TotalAmount == 0))
			{
				targetCollection.Clear();
				return;
			}

			if (targetCollection.Count == 0)
			{
				// Если коллекция пуста, создаем новую серию
				var series = CreateNewSeries(transactionType);
				targetCollection.Add(series);
			}

			if (targetCollection[0] is LineSeries lineSeries)
			{
				// Обновляем данные в этой серии
				UpdateSeriesData(lineSeries, chartData);
			}

			// Обновляем лейблы и форматтеры
			UpdateLabelsAndFormatters(chartData, transactionType);
		});
	}

	private static void UpdateSeriesData(LineSeries series, IReadOnlyList<(DateTime Month, decimal TotalAmount, decimal Percentage)> chartData)
	{
		series.Values.Clear();

		for (var i = 0; i < chartData.Count; i++)
		{
			var dataPoint = new ChartDataPoint(chartData[i].TotalAmount, $"{chartData[i].Percentage:F0}%");
			series.Values.Add(dataPoint);
		}

	}

	private static LinearGradientBrush CreateGradientBrush(string transactionType)
	{
		Color startColor, oneMiddleColor, twoMiddleColor, endColor;
		if (transactionType == "ДОХОДЫ")
		{
			// Зеленый градиент для доходов
			startColor = Color.FromArgb(20, 76, 175, 80);
			oneMiddleColor = Color.FromArgb(13, 76, 175, 80);
			twoMiddleColor = Color.FromArgb(7, 76, 175, 80);
			endColor = Color.FromArgb(0, 76, 175, 80);
		}
		else
		{
			// Красный градиент для расходов
			startColor = Color.FromArgb(20, 255, 62, 88);
			oneMiddleColor = Color.FromArgb(13, 255, 62, 88);
			twoMiddleColor = Color.FromArgb(7, 255, 62, 88);
			endColor = Color.FromArgb(0, 255, 62, 88);
		}

		return new LinearGradientBrush
		{
			StartPoint = new Point(0.5, 0),
			EndPoint = new Point(0.5, 1),
			GradientStops =
			[
				new GradientStop(startColor, 0),
				new GradientStop(oneMiddleColor, 0.25),
				new GradientStop(twoMiddleColor, 0.72),
				new GradientStop(endColor, 1)
			]
		};
	}

	private static LineSeries CreateNewSeries(string transactionType)
	{

		var strokeColor = transactionType == "ДОХОДЫ" ? "#3CB371" : "#FF3E58";
		return new LineSeries
		{
			Values = new ChartValues<ChartDataPoint>(),
			Fill = CreateGradientBrush(transactionType),
			Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(strokeColor)),
			PointForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(strokeColor)),
			PointGeometrySize = 12,
			StrokeThickness = 2,
			Opacity = 0.4,
		};
	}

	private void UpdateLabelsAndFormatters(List<(DateTime Month, decimal TotalAmount, decimal Percentage)> chartData, string transactionType)
	{
		var labels = chartData.Select(x => x.Month.ToString("MMM").ToUpper()[..3]).ToArray();
		var yFormatter = new Func<double, string>(value => value.ToString("N0"));

		if (transactionType == "ДОХОДЫ")
		{
			IncomeLabels = labels;
			IncomeYFormatter = yFormatter;
		}
		else // По умолчанию обрабатываем как расходы
		{
			ExpenseLabels = labels;
			ExpenseYFormatter = yFormatter;
		}
	}

	private static async Task<decimal> FetchBalanceForCurrentMonthAsync()
	{
		await using var context = new DashboardDbContext();

		var currentDate = DateTime.Now;
		var currentMonth = currentDate.Month;
		var currentYear = currentDate.Year;

		var balanceForCurrentMonth = await context.Balances
			.Where(b => b.Year == currentYear && b.Month == currentMonth)
			.Select(b => b.BalanceAmount)
			.FirstOrDefaultAsync();

		if (balanceForCurrentMonth == default)
		{
			balanceForCurrentMonth = await context.Balances
				.OrderByDescending(b => b.Year)
				.ThenByDescending(b => b.Month)
				.Select(b => b.BalanceAmount)
				.FirstOrDefaultAsync();
		}

		return balanceForCurrentMonth;
	}

	private async void UpdateBalanceForCurrentMonth()
	{	
		var balanceForCurrentMonth = await FetchBalanceForCurrentMonthAsync();
		var ci = new CultureInfo("ru-RU");
		TotalBalance = balanceForCurrentMonth.ToString("N2", ci) + " ₽";
	}

	private async Task<List<(string MonthName, decimal Balance)>> FetchBalanceChartDataAsync()
	{
		await using var context = new DashboardDbContext();
		var selectedYears = Years.Where((year, index) => SelectedYears.Contains(index)).ToList();

		var now = DateTime.Now;
		var balances = new List<Balance>();

		// Проверка на выбор текущего и предыдущего года одновременно
		var isPreviousYearSelected = selectedYears.Any(year => year == (now.Year - 1).ToString());
		var isCurrentYearSelected = selectedYears.Any(year => year == now.Year.ToString());

		// Проверка для обработки случая с несколькими выбранными годами, исключая текущий год
		var isMultipleYearsExcludingCurrentSelected = selectedYears.Count > 1 && !selectedYears.Contains(now.Year.ToString());

		if (isMultipleYearsExcludingCurrentSelected)
		{
			var minYear = selectedYears.Min(int.Parse);
			var maxYear = selectedYears.Max(int.Parse);

			balances.AddRange(await context.Balances
				.Where(b => b.Year >= minYear && b.Year <= maxYear)
				.ToListAsync());
		}

		else if (isPreviousYearSelected && isCurrentYearSelected)
		{
			// Особый случай: выбраны и текущий, и предыдущий годы
			var startPeriod = new DateTime(now.Year - 1, 1, 1); // Январь предыдущего года
			var endPeriod = now; // Текущий месяц текущего года

			balances = await context.Balances
				.Where(b =>
					(b.Year > startPeriod.Year || (b.Year == startPeriod.Year && b.Month >= startPeriod.Month)) &&
					(b.Year < endPeriod.Year || (b.Year == endPeriod.Year && b.Month <= endPeriod.Month)))
				.ToListAsync();
		}
		else
		{
			// Обработка каждого выбранного года по отдельности
			foreach (var year in selectedYears)
			{
				if (year == now.Year.ToString())
				{
					// Для текущего года выбираем данные за последние 12 месяцев
					var startDate = now.AddYears(-1);

					balances.AddRange(await context.Balances
						.Where(b =>
							(b.Year > startDate.Year || (b.Year == startDate.Year && b.Month >= startDate.Month)) &&
							(b.Year < now.Year || (b.Year == now.Year && b.Month <= now.Month)))
						.ToListAsync());
				}
				else
				{
					// Для всех остальных лет выбираем данные за весь год
					balances.AddRange(await context.Balances
						.Where(b => b.Year.ToString() == year)
						.ToListAsync());
				}
			}
		}

		// Сортировка и преобразование данных для графика
		var balanceChartData = balances
			.OrderBy(b => b.Year).ThenBy(b => b.Month)
			.Select(b => (Month: b.MonthName, Balance: b.BalanceAmount))
			.ToList();

		return balanceChartData;
	}

	private async void UpdateBalanceChartData()
	{
		var balanceData = await FetchBalanceChartDataAsync();

		Application.Current.Dispatcher.Invoke(() =>
		{
			if (BalanceChartSeries.Count == 0)
			{
				// Если коллекция пуста, создаем новую серию
				var series = CreateNewSeriesForBalance(); 
				BalanceChartSeries.Add(series);
			}

			if (BalanceChartSeries[0] is ColumnSeries columnSeries)
			{
				// Обновляем данные в этой серии
				UpdateSeriesDataForBalance(columnSeries, balanceData); // Обновляем данные для столбчатой серии
			}
		});
	}

	private static ColumnSeries CreateNewSeriesForBalance()
	{
		var series = new ColumnSeries
		{
			Values = new ChartValues<decimal>(),
			Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#01A4F6")),
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9")),
			DataLabels = true,
			MaxColumnWidth = 28,
			FontSize = 11,
		};

		// Определение шаблона меток с кастомным отступом
		var dataTemplate = new DataTemplate();

		var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
		textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Point.Y") { StringFormat = "N0", ConverterCulture = new CultureInfo("ru-RU") });
		textBlockFactory.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Oswald Regular"));
		textBlockFactory.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 0, 10));
		textBlockFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);

		var rotateTransform = new RotateTransform(90);
		textBlockFactory.SetValue(TextBlock.LayoutTransformProperty, rotateTransform);
		textBlockFactory.SetValue(TextBlock.RenderTransformOriginProperty, new Point(0.5, 0.5));

		dataTemplate.VisualTree = textBlockFactory;

		series.DataLabelsTemplate = dataTemplate;
		return series;
	}

	private void UpdateSeriesDataForBalance(ColumnSeries series, List<(string Month, decimal Balance)> balanceData)
	{
		series.Values.Clear();
		var yFormatter = new Func<double, string>(value => value.ToString("N0"));

		foreach (var item in balanceData)
		{
			series.Values.Add(item.Balance);
		}

		BalanceLabels = balanceData.Select(x => x.Month.ToUpper()[..3]).ToArray();
		BalanceYFormatter = yFormatter;
	}

	private static void MapChart() //Простой маппер который позволит пихать в ChartValues тип ChartDataPoint
	{
		var mapper = Mappers.Xy<ChartDataPoint>()
		.X((value, index) => index) // Используем индекс точки как X значение
		.Y(value => (double)value.Value); // Используем Value как Y значение

		Charting.For<ChartDataPoint>(mapper);
	}
	#endregion

	#region Обновление диаграммы
	private async void UpdatePieChartAsync(string transactionType, List<string> selectedCategories, SeriesCollection collection)
	{
		await using var context = new DashboardDbContext();

		// Определение выбранных лет для фильтрации данных
		var selectedYears = Years.Where((year, index) => SelectedYears.Contains(index)).ToList();

		var selectedSum = 0m;

		var hasData = await context.Transactions
			.AnyAsync(t => selectedYears.Contains(t.Date.Year.ToString()) && t.Type == transactionType);

		if (transactionType == "РАСХОДЫ")
		{
			HasExpenseData = hasData;
			IsChartExpenseAvailable = hasData;
		}
		else
		{
			HasIncomeData = hasData;
			IsChartIncomeAvailable = hasData;
		}
		
		// Получение общей суммы по выбранным годам для заданного типа транзакции
		var totalSum = (decimal)context.Transactions
			.Where(t => t.Type == transactionType)
			.Sum(t => (double)t.Amount);

		Application.Current.Dispatcher.Invoke(() =>
		{
			foreach (var series in collection)
			{
				if (series is PieSeries pieSeries)
				{
					pieSeries.Fill.Opacity = hasData ? 0.9 : 0.2; // Установка прозрачности в зависимости от наличия данных
				}
			}
		});

		if (hasData)
		{
			foreach (var seriesView in collection)
			{
				var series = (PieSeries)seriesView;
				// Сумма по категории и выбранным годам
				var sum = (decimal)context.Transactions
					.Where(t => t.Category == series.Title && selectedYears.Contains(t.Date.Year.ToString()) && t.Type == transactionType)
					.Sum(t => (double)t.Amount);

				if (selectedCategories.Contains(series.Title))
				{
					selectedSum += sum; // Суммируем только выбранные категории
				}

				series.Values[0] = Math.Max(sum, 0.1m); // 0.1 как минимальное значение для отображения
				series.Fill.Opacity = selectedCategories.Contains(series.Title) && sum > 0 ? 0.9 : 0.35;
			}

			var ci = new CultureInfo("ru-RU");
			var selectedSumFormatted = selectedSum.ToString("N0", ci);
			var totalSumFormatted = totalSum.ToString("N2", ci) + " ₽";
			var percentage = totalSum > 0 ? $"{(selectedSum / totalSum * 100):F0}%" : "0%";

			// Обновление UI в зависимости от типа транзакции
			if (transactionType == "РАСХОДЫ")
			{
				SelectedExpenseSum = selectedSumFormatted;
				TotalExpenseSum = totalSumFormatted;
				PercentageExpense = percentage;
				UpdateChartData(ChartExpenseCollection, "РАСХОДЫ");
			}
			else
			{
				SelectedIncomeSum = selectedSumFormatted;
				TotalIncomeSum = totalSumFormatted;
				PercentageIncome = percentage;
				UpdateChartData(ChartIncomeCollection, "ДОХОДЫ");
			}
		}
		else
		{
			UpdateChartData(ChartExpenseCollection, "РАСХОДЫ");
			UpdateChartData(ChartIncomeCollection, "ДОХОДЫ");

			UpdateChartLabelsForNoData(transactionType);
		}

		UpdateBalanceChartData();
		UpdateBalanceForCurrentMonth();
		CalculateQuarterlyBalancesAsync();

	}

	//Метод для выбора элемента в листбокс при клике на серию диаграммы
	private void SelectCategory(string category, bool isExpense)
	{
		var categories = isExpense ? ExpenseCategories : IncomeCategories;
		var selectedCategories = isExpense ? SelectedExpenses : SelectedIncomes;

		var categoryIndex = categories.IndexOf(category);
		if (categoryIndex < 0) return;

		selectedCategories.Clear();
		selectedCategories.Add(categoryIndex);

		// Вызов универсального метода обновления диаграммы
		if (isExpense)
		{
			UpdatePieChartAsync("РАСХОДЫ", ExpenseCategories.Where((c, i) => SelectedExpenses.Contains(i)).ToList(), ExpenseCollection);
		}
		else
		{
			UpdatePieChartAsync("ДОХОДЫ", IncomeCategories.Where((c, i) => SelectedIncomes.Contains(i)).ToList(), IncomeCollection);
		}
	}
	#endregion

	#region Логика множественного и одиночного выделения ListBox
	private ObservableCollection<int>? GetTargetCollection(ListBox listBox)
	{
		switch (listBox.Name)
		{
			case "ExpenseLb":
				return SelectedExpenses;
			case "YearLb":
				return SelectedYears;
			case "IncomeLb":
				return SelectedIncomes;
			default:
				return null; // Если ListBox не распознан
		}
	}

	private void UpdateSelection(ListBox listBox, Rect selectionRect, bool isSimpleClick = false)
	{
		var targetCollection = GetTargetCollection(listBox);

		if (isSimpleClick)
		{
			var selectedIndex = listBox.SelectedIndex;
			if (selectedIndex == -1) return; // Проверяем, что выбран действительный элемент

			targetCollection.Clear();
			targetCollection.Add(selectedIndex);
		}
		else
		{
			targetCollection.Clear();

			for (var i = 0; i < listBox.Items.Count; i++)
			{
				var item = listBox.Items[i];

				if (listBox.ItemContainerGenerator.ContainerFromItem(item) is not ListBoxItem) continue;
				if (IsItemIntersectsWithSelectionRect(listBox, i, selectionRect))
					targetCollection.Add(i);
			}

		}
	}

	private static bool IsItemIntersectsWithSelectionRect(ListBox listBox, int itemIndex, Rect selectionRect)
	{
		if (listBox.ItemContainerGenerator.ContainerFromIndex(itemIndex) is not ListBoxItem listBoxItem) return false;

		var itemPosition = listBoxItem.TranslatePoint(new Point(0, 0), listBox);
		var itemRect = new Rect(itemPosition, new Size(listBoxItem.ActualWidth, listBoxItem.ActualHeight));
		return selectionRect.IntersectsWith(itemRect);
	}
	#endregion
}