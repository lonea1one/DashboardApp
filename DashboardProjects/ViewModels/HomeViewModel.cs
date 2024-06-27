using DashboardProjects.Commands;
using DashboardProjects.Services;
using DashboardProjects.Utils;
using DataAccess;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<HomeViewModel> _logger;
    private readonly DataService _dataService;
    private readonly FileProcessingService _fileProcessingService;

    private INavigationService _navigationService { get; set; }
    public ObservableCollection<string>? ExpenseCategories { get; set; }
	
	public ObservableCollection<string>? IncomeCategories { get; set; }
	
	public ObservableCollection<string>? Years { get; set; }
	
	public ObservableCollection<int>? SelectedExpenses { get; set; }
	
	public ObservableCollection<int>? SelectedIncomes { get; set; }
	
	public ObservableCollection<int>? SelectedYears { get; set; }

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
	
	public string? SelectedExpenseSum { get; set; }
	
	public string? SelectedIncomeSum { get; set; }
	
	public string? TotalExpenseSum { get; set; }
	
	public string? TotalBalance { get; set; }
	
	public string? TotalIncomeSum { get; set; }
	
	public string? PercentageExpense { get; set; }
	
	public string? PercentageIncome { get; set; }
	
	public string? FirstQuartalBalance { get; set; }
	
	public string? SecondQuartalBalance { get; set; }
	
	public string? ThirdQuartalBalance { get; set; }
	
	public string? FourdQuartalBalance { get; set; }
	
	public string[]? ExpenseLabels { get; set; }
	
	public string[]? IncomeLabels { get; set; }
	
	public string[]? BalanceLabels { get; set; }
	
	public string SelectedExpenseAmount { get; set; }
	
	public string SelectedIncomeAmount { get; set; }
	
	public string SelectedPercentageExpense { get; set; }
	
	public string SelectedPercentageIncome { get; set; }

	
	public Point StartPoint { get; set; }
	
	public bool IsSelecting { get; set; }
	
	public bool HasExpenseData { get; set; }

	private bool _hasIncomeData;
	public bool HasIncomeData
	{
		get => _hasIncomeData;
		set
		{
			if (_hasIncomeData == value) return;
			_hasIncomeData = value;
			OnPropertyChanged();
		}
	}
	
	public bool IsChartIncomeAvailable { get; set; }
	
	public bool IsChartExpenseAvailable { get; set; }

	private Visibility _progressVisibility = Visibility.Collapsed;
	public Visibility ProgressVisibility
	{
		get => _progressVisibility;
		set
		{
			if (_progressVisibility == value) return;
			_progressVisibility = value;
			OnPropertyChanged();
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
			OnPropertyChanged();
		}
	}
	
	public bool MouseMoved { get; set; }

	public HomeViewModel(INavigationService navigationService, FileProcessingService fileProcessingService, DataService dataService, ILogger<HomeViewModel> logger)
	{
        _logger = logger;
        _fileProcessingService = fileProcessingService;
        _dataService = dataService;
        _navigationService = navigationService;

        _logger.LogInformation("Создание экземпляра HomeViewModel.");

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

        _logger.LogInformation("Начало выделения элементов в списке.");
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

        _logger.LogInformation("Обновление выделенных элементов в списке.");
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
                _logger.LogInformation("Выбраны расходы для обновления диаграммы.");
                break;
			case "IncomeLb":
				UpdatePieChartAsync("ДОХОДЫ", IncomeCategories.Where((category, index) => SelectedIncomes.Contains(index)).ToList(), IncomeCollection);
                _logger.LogInformation("Выбраны доходы для обновления диаграммы.");
                break;
			case "YearLb":
                // Если необходимо выполнить общее действие для всех других случаев
                _logger.LogInformation("Выбраны годы для обновления диаграммы.");
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
        var transactionType = isExpense ? "РАСХОДЫ" : "ДОХОДЫ";

        _logger.LogInformation($"Выбрана категория для типа транзакции: {transactionType}");
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

	private void OnDragOver(object parameter)
	{
		var e = parameter as DragEventArgs;

		e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
		e.Handled = true;

        _logger.LogInformation("Перетаскивание файла над областью для загрузки.");
    }

	private async void OnFileDrop(object parameter)
	{
		if (parameter is not DragEventArgs args) return;

		if (!args.Data.GetDataPresent(DataFormats.FileDrop)) return;
		var files = (string[])args.Data.GetData(DataFormats.FileDrop);
		if (files.Length <= 0 || !Path.GetExtension(files[0]).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)) return;

        _logger.LogInformation($"Начало обработки файла: {files[0]}");

        UploadVisibility = Visibility.Collapsed;
		ProgressVisibility = Visibility.Visible; // Показываем ProgressBar

		try
		{
            // Передаем путь к файлу и ожидаем завершения асинхронной операции
            await _fileProcessingService.ProcessFileAsync(files[0]);
            Application.Current.Dispatcher.Invoke(InitializeSelectedItems);

            _logger.LogInformation($"Файл {files[0]} успешно обработан.");
        }
		catch (Exception ex)
		{
            _logger.LogError(ex, "Произошла ошибка при обработке файла");
            MessageBox.Show($"Произошла ошибка при обработке файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		finally
		{
			// Скрываем ProgressBar и возвращаем иконку и надписи
			ProgressVisibility = Visibility.Collapsed;
			UploadVisibility = Visibility.Visible;

            _logger.LogInformation("Завершение обработки файла.");
        }
	}
	#endregion

	#region Асинхронная загрузка данных
	private async Task LoadDataAsync()
	{
        _logger.LogInformation("Начало загрузки данных.");

        await using var context = new DashboardDbContext();
		var expenses = await _dataService.GetExpenseCategoriesAsync();
		var incomes = await _dataService.GetIncomeCategoriesAsync();
		var years = await _dataService.GetYearsAsync();

		// Обновление UI происходит в потоке UI
		Application.Current.Dispatcher.Invoke(() => UpdateUi(expenses, incomes, years));

        _logger.LogInformation("Загрузка данных завершена.");
    }

	private void UpdateUi(List<string> expenses, List<string> incomes, List<string> years)
	{
        _logger.LogInformation("Обновление пользовательского интерфейса.");

        ExpenseCategories = new ObservableCollection<string>(expenses);
		IncomeCategories = new ObservableCollection<string>(incomes);
		Years = new ObservableCollection<string>(years);
		InitializeSelectedItems();
	}
	#endregion

	#region Расчет кварталов
	private async void CalculateQuarterlyBalancesAsync()
	{
        try
        {
            _logger.LogInformation("Начало расчета квартальных балансов.");

            var selectedYearValues = SelectedYears.Select(index => Years[index]).ToList();

            await using var context = new DashboardDbContext();
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

            FirstQuartalBalance = SecondQuartalBalance = ThirdQuartalBalance = FourdQuartalBalance = "0";

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

            _logger.LogInformation("Расчет квартальных балансов успешно завершен.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при расчете квартальных балансов.");
        }
    }

	private string UpdateQuartalBalance(string currentBalance, string newBalance)
	{
        try
        {
            var current = decimal.Parse(currentBalance);
            var update = decimal.Parse(newBalance);
            return (current + update).ToString("N0");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении квартального баланса.");
            return currentBalance;
        }
    }
	#endregion

	#region Инициализация диаграммы и графика
	private async void InitializeSelectedItems()
	{
        try
        {
            _logger.LogInformation("Инициализация выбранных элементов.");

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

            await InitializePieChartSeries("РАСХОДЫ", ExpenseCollection, GetExpenseChartColors());
            await InitializePieChartSeries("ДОХОДЫ", IncomeCollection, GetIncomeChartColors());

            _logger.LogInformation("Инициализация выбранных элементов завершена.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при инициализации выбранных элементов.");
        }
    }

	private async Task InitializePieChartSeries(string type, SeriesCollection collection, IReadOnlyList<Brush> colors)
	{
        try
        {
            _logger.LogInformation($"Инициализация диаграммы для типа транзакции: {type}");

            var data = await _dataService.GetCategorySumAsync(type);
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

            _logger.LogInformation($"Инициализация диаграммы для типа транзакции {type} успешно завершена.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при инициализации диаграммы для типа транзакций {type}.");
        }
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
	private async void UpdateChartData(SeriesCollection targetCollection, string transactionType)
	{
        try
        {
            _logger.LogInformation($"Начало обновления данных для графика {transactionType}");

            var selectedCategories = transactionType == "РАСХОДЫ"
                ? ExpenseCategories.Where((category, index) => SelectedExpenses.Contains(index)).ToList()
                : IncomeCategories.Where((category, index) => SelectedIncomes.Contains(index)).ToList();

            var selectedYears = Years.Where((year, index) => SelectedYears.Contains(index)).ToList();

            var chartData = await _dataService.FetchChartDataAsync(transactionType, selectedCategories, selectedYears);

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (chartData.Count == 0 || chartData.All(x => x.TotalAmount == 0))
                {
                    targetCollection.Clear();
                    return;
                }

                if (targetCollection.Count == 0)
                {
                    var series = CreateNewSeries(transactionType);
                    targetCollection.Add(series);
                }

                if (targetCollection[0] is LineSeries lineSeries)
                {
                    UpdateSeriesData(lineSeries, chartData);
                }

                UpdateLabelsAndFormatters(chartData, transactionType);
            });

            _logger.LogInformation($"Обновление данных для графика {transactionType} успешно завершено.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при обновлении данных для графика {transactionType}.");
            // Можно добавить дополнительную обработку ошибки здесь, если необходимо
        }
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

	private async void UpdateBalanceForCurrentMonth()
	{
        try
        {
            _logger.LogInformation("Начало обновления баланса за текущий месяц.");

            var balanceForCurrentMonth = await _dataService.FetchBalanceForCurrentMonthAsync();
            var ci = new CultureInfo("ru-RU");
            TotalBalance = balanceForCurrentMonth.ToString("N2", ci) + " ₽";

            _logger.LogInformation("Обновление баланса за текущий месяц успешно завершено.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении баланса за текущий месяц.");
        }
    }

	private async void UpdateBalanceChartData()
	{
        try
        {
            _logger.LogInformation("Начало обновления данных для диаграммы баланса.");

            var selectedYears = Years.Where((year, index) => SelectedYears.Contains(index)).ToList();
            var balanceData = await _dataService.FetchBalanceChartDataAsync(selectedYears);

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (BalanceChartSeries.Count == 0)
                {
                    var series = CreateNewSeriesForBalance();
                    BalanceChartSeries.Add(series);
                }

                if (BalanceChartSeries[0] is ColumnSeries columnSeries)
                {
                    UpdateSeriesDataForBalance(columnSeries, balanceData);
                }
            });

            _logger.LogInformation("Обновление данных для диаграммы баланса успешно завершено.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении данных для диаграммы баланса.");
        }
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
        try
        {
            _logger.LogInformation($"Начало обновления диаграммы {transactionType}");

            var selectedYears = Years.Where((year, index) => SelectedYears.Contains(index)).ToList();
            var selectedSum = 0m;

            await using var context = new DashboardDbContext();
            var hasData = await context.Transactions
                .AnyAsync(t => selectedYears.Contains(t.Date.Year.ToString()) && t.Type == transactionType);

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var series in collection)
                {
                    if (series is PieSeries pieSeries)
                    {
                        pieSeries.Fill.Opacity = hasData ? 0.9 : 0.2;
                    }
                }
            });

            if (hasData)
            {
                foreach (var seriesView in collection)
                {
                    var series = (PieSeries)seriesView;
                    var sum = (decimal)context.Transactions
                        .Where(t => t.Category == series.Title && selectedYears.Contains(t.Date.Year.ToString()) && t.Type == transactionType)
                        .Sum(t => (double)t.Amount);

                    if (selectedCategories.Contains(series.Title))
                    {
                        selectedSum += sum;
                    }

                    series.Values[0] = Math.Max(sum, 0.1m); // Минимальное значение для отображения
                    series.Fill.Opacity = selectedCategories.Contains(series.Title) && sum > 0 ? 0.9 : 0.35;
                }

                var ci = new CultureInfo("ru-RU");
                var totalSum = (decimal)context.Transactions
                    .Where(t => t.Type == transactionType)
                    .Sum(t => (double)t.Amount);

                var selectedSumFormatted = selectedSum.ToString("N0", ci);
                var totalSumFormatted = totalSum.ToString("N2", ci) + " ₽";
                var percentage = totalSum > 0 ? $"{(selectedSum / totalSum * 100):F0}%" : "0%";

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

            _logger.LogInformation($"Обновление диаграммы {transactionType} успешно завершено.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при обновлении диаграммы {transactionType}.");
        }
    }

	//Метод для выбора элемента в листбокс при клике на серию диаграммы
	private void SelectCategory(string category, bool isExpense)
	{
        try
        {
            _logger.LogInformation($"Выбрана категория {category} для {(isExpense ? "расходов" : "доходов")}");

            var categories = isExpense ? ExpenseCategories : IncomeCategories;
            var selectedCategories = isExpense ? SelectedExpenses : SelectedIncomes;

            var categoryIndex = categories.IndexOf(category);
            if (categoryIndex < 0) return;

            selectedCategories.Clear();
            selectedCategories.Add(categoryIndex);

            if (isExpense)
            {
                UpdatePieChartAsync("РАСХОДЫ", ExpenseCategories.Where((c, i) => SelectedExpenses.Contains(i)).ToList(), ExpenseCollection);
            }
            else
            {
                UpdatePieChartAsync("ДОХОДЫ", IncomeCategories.Where((c, i) => SelectedIncomes.Contains(i)).ToList(), IncomeCollection);
            }

            _logger.LogInformation($"Обновление диаграммы после выбора категории {category} для {(isExpense ? "расходов" : "доходов")} успешно завершено.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при выборе категории для {(isExpense ? "расходов" : "доходов")}.");
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