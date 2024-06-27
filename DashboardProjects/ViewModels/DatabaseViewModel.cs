using DashboardProjects.Commands;
using DataAccess.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DashboardProjects.Utils;
using DashboardProjects.Views;
using Microsoft.Extensions.Logging;
using DashboardProjects.Services;

namespace DashboardProjects.ViewModels;

public partial class DatabaseViewModel : BaseViewModel
{
    private readonly ILogger<DatabaseViewModel> _logger;
    private readonly TransactionService _transactionService;
    private INavigationService _navigationService { get; set; }

    public ObservableCollection<string> Years { get; set; }
	public ObservableCollection<int> SelectedYears { get; set; }
	public ObservableCollection<Transaction> Transactions { get; set; }
    public ObservableCollection<Transaction> FilteredTransactions { get; set; }

    private Transaction _selectedTransaction;
	public Transaction SelectedTransaction
	{
		get => _selectedTransaction;
		set
		{
			_selectedTransaction = value;
			OnPropertyChanged();
			IsEditEnabled = _selectedTransaction != null;
			IsDeleteEnabled = _selectedTransaction != null;
		}
	}
	
	public ICommand LbMouseLeftButtonDownCommand { get; private set; }
	public ICommand DataGridSelectionChangedCommand { get; private set; }
	public ICommand LbMouseMoveCommand { get; private set; }
	public ICommand LbMouseLeftButtonUpCommand { get; private set; }
	public ICommand AddNewItemWindowCommand { get; }
	public ICommand DeleteCommand { get; }
	public ICommand EditCommand { get; }
	
	
	public bool IsEditEnabled { get; set; }
	public bool IsDeleteEnabled { get; set; }
	public Point StartPoint { get; set; }
	public bool IsSelecting { get; set; }
	public bool MouseMoved { get; set; }
	private string _searchText;
	public string SearchText
	{
		get => _searchText;
		set
		{
			_searchText = value;
			OnPropertyChanged();
			ApplyFilters();
		}
	}

	[System.Text.RegularExpressions.GeneratedRegex(@"^[0-9.]+$")]
	private static partial System.Text.RegularExpressions.Regex MyRegex();

	public DatabaseViewModel(INavigationService navigationService, TransactionService transactionService, ILogger<DatabaseViewModel> logger)
	{
        _logger = logger;
		_transactionService = transactionService;
		_navigationService = navigationService;

        _logger.LogInformation("Создание экземпляра DatabaseViewModel.");

        Years = [];
		SelectedYears = [];

		LbMouseLeftButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(OnPreviewMouseLeftButtonDown);
		LbMouseMoveCommand = new RelayCommand<MouseEventArgs>(OnPreviewMouseMove);
		LbMouseLeftButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(OnPreviewMouseLeftButtonUp);
		
		DataGridSelectionChangedCommand = new RelayCommand(OnDataGridSelectionChanged);

		EditCommand = new RelayCommand(EditTransaction);
		DeleteCommand = new RelayCommand(DeleteTransaction);
		AddNewItemWindowCommand = new RelayCommand(OnOpenAddNewItemWindowCommand);

		_ = LoadDataAsync();

		EventMediator.TransactionAdded += OnTransactionAdded;
	}

	private async Task LoadDataAsync()
	{
        _logger.LogInformation("Загрузка данных начата.");

		try
		{
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var years = await _transactionService.GetDistinctYearsAsync();
            UpdateUi(years, transactions);
            _logger.LogInformation("Данные успешно загружены.");
        }
		catch (Exception ex)
		{
            _logger.LogError(ex, "Ошибка при загрузке данных из БД.");
            _logger.LogCritical(ex, "Критическая ошибка при загрузке данных. Приложение не может продолжать работу.");
        }
	}

	private void OnTransactionAdded(object sender, EventArgs e)
	{
        _logger.LogInformation("Добавлена новая транзакция. Обновление данных.");
        _ = LoadDataAsync();
		ApplyFilters();
	}

	private void UpdateUi(List<string> years, List<Transaction> transactions)
	{
		Application.Current.Dispatcher.InvokeAsync(() =>
		{
			Years = new ObservableCollection<string>(years);
			Transactions = new ObservableCollection<Transaction>(transactions);
			FilteredTransactions = new ObservableCollection<Transaction>(transactions);
			InitializeSelectedItems();
		});
	}

	private void InitializeSelectedItems()
	{
		SelectedYears?.Clear();

		for (var i = 0; i < Years?.Count; i++) SelectedYears?.Add(i);

        _logger.LogInformation("Инициализация выбранных элементов завершена.");
    }

	private async void ApplyFilters()
	{
        _logger.LogInformation("Применение фильтров к транзакциям.");

        if (Transactions == null || SelectedYears == null)
		{
			FilteredTransactions = Transactions;
			return;
		}

		var selectedYears = SelectedYears.Select(index => Years[index]).ToList();
		var searchTextLower = SearchText?.ToLower();

        try
        {
            var filteredTransactions = await Task.Run(() =>
            {
                return Transactions
                    .Where(transaction =>
                        selectedYears.Contains(transaction.Date.Year.ToString()) &&
                        (string.IsNullOrEmpty(SearchText) ||
                         transaction.Date.ToString().ToLower().Contains(searchTextLower) ||
                         transaction.Type.ToLower().Contains(searchTextLower) ||
                         transaction.Amount.ToString().ToLower().Contains(searchTextLower) ||
                         transaction.Category.ToLower().Contains(searchTextLower)))
                    .ToList();
            });

            FilteredTransactions = new ObservableCollection<Transaction>(filteredTransactions);

            _logger.LogInformation("Фильтры успешно применены.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при применении фильтров к транзакциям.");
        }
    }

	private void OnDataGridSelectionChanged(object parameter)
	{
		var args = parameter as SelectionChangedEventArgs;
		if (args?.Source is not DataGrid dataGrid) return;
		
		SelectedTransaction = dataGrid.SelectedItem as Transaction;
        _logger.LogInformation("Изменен выбранный элемент в DataGrid.");
    }
	
	private void OnPreviewMouseLeftButtonDown(object parameter)
	{
		var args = parameter as MouseButtonEventArgs;
		if (args?.Source is not ListBox listBox) return;

		var isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
		var isAltPressed = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;

		if (isCtrlPressed || isAltPressed) return;
		StartPoint = args.GetPosition(listBox);
		IsSelecting = true;

        _logger.LogInformation("Начато выделение мышью в ListBox.");
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

        _logger.LogInformation("Процесс выделения мышью в ListBox продолжается.");
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

		ApplyFilters();
        _logger.LogInformation("Завершено выделение мышью в ListBox.");


    }
	
	private void OnOpenAddNewItemWindowCommand(object parameter)
	{
        _logger.LogInformation("Открытие окна для добавления новой транзакции.");
        var addTransactionView = new AddTransactionView(_logger);
		addTransactionView.ShowDialog();
	}

	private void EditTransaction(object parameter)
	{
		if (SelectedTransaction == null) return;

        _logger.LogInformation($"Редактирование транзакции с ID: {SelectedTransaction.Id}");
        var addTransactionWindow = new AddTransactionView(SelectedTransaction, _logger);
		addTransactionWindow.ShowDialog();
	}

	private async void DeleteTransaction(object parameter)
	{
		if (SelectedTransaction == null) return;
		var confirmation = MessageBox.Show("Вы уверены что хотите удалить выделенную транзакцию?", "Удалить транзакцию", MessageBoxButton.YesNo, MessageBoxImage.Question);

		if (confirmation != MessageBoxResult.Yes) return;

        try
        {
            await _transactionService.DeleteTransactionAsync(SelectedTransaction.Id);
            _logger.LogInformation($"Транзакция с ID: {SelectedTransaction.Id} успешно удалена.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при удалении транзакции с ID: {SelectedTransaction.Id}.");
        }
    }

	private void UpdateSelection(ListBox listBox, Rect selectionRect, bool isSimpleClick = false)
	{
		var targetCollection = SelectedYears;

		if (isSimpleClick)
		{
			var selectedIndex = listBox.SelectedIndex;
			if (selectedIndex == -1) return;

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

        _logger.LogInformation("Обновление выделения элементов в ListBox завершено.");
    }

	private static bool IsItemIntersectsWithSelectionRect(ListBox listBox, int itemIndex, Rect selectionRect)
	{
		if (listBox.ItemContainerGenerator.ContainerFromIndex(itemIndex) is not ListBoxItem listBoxItem) return false;

		var itemPosition = listBoxItem.TranslatePoint(new Point(0, 0), listBox);
		var itemRect = new Rect(itemPosition, new Size(listBoxItem.ActualWidth, listBoxItem.ActualHeight));
		return selectionRect.IntersectsWith(itemRect);
	}
}