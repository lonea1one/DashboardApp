using DashboardProjects.Commands;
using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DashboardProjects.Utils;
using DashboardProjects.Views;

namespace DashboardProjects.ViewModels;

public class DatabaseViewModel : BaseViewModel
{
	private ObservableCollection<string> _years;
	public ObservableCollection<string> Years
	{
		get => _years;
		set
		{
			_years = value;
			OnPropertyChanged(nameof(Years));
		}
	}

	private ObservableCollection<int> _selectedYears;
	public ObservableCollection<int> SelectedYears
	{
		get => _selectedYears;
		set
		{
			_selectedYears = value;
			OnPropertyChanged(nameof(SelectedYears));
			ApplyFilters();
		}
	}

	private ObservableCollection<Transaction> _transactions;
	public ObservableCollection<Transaction> Transactions
	{
		get => _transactions;
		set
		{
			_transactions = value;
			OnPropertyChanged(nameof(Transactions));
		}
	}

	private Transaction _selectedTransaction;
	public Transaction SelectedTransaction
	{
		get => _selectedTransaction;
		set
		{
			_selectedTransaction = value;
			OnPropertyChanged(nameof(SelectedTransaction));
			IsEditEnabled = _selectedTransaction != null;
			IsDeleteEnabled = _selectedTransaction != null;
		}
	}

	private ObservableCollection<Transaction> _filteredTransactions;
	public ObservableCollection<Transaction> FilteredTransactions
	{
		get => _filteredTransactions;
		set
		{
			_filteredTransactions = value;
			OnPropertyChanged(nameof(FilteredTransactions));
		}
	}

	public ICommand LbMouseLeftButtonDownCommand { get; private set; }
	public ICommand LbMouseMoveCommand { get; private set; }
	public ICommand LbMouseLeftButtonUpCommand { get; private set; }
	public ICommand AddNewItemWindowCommand { get; }
	public ICommand DeleteCommand { get; }
	public ICommand EditCommand { get; }

	private bool _isEditEnabled;
	public bool IsEditEnabled
	{
		get => _isEditEnabled;
		set
		{
			_isEditEnabled = value;
			OnPropertyChanged(nameof(IsEditEnabled));
		}
	}

	private bool _isDeleteEnabled;
	public bool IsDeleteEnabled
	{
		get => _isDeleteEnabled;
		set
		{
			_isDeleteEnabled = value;
			OnPropertyChanged(nameof(IsDeleteEnabled));
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

	private string _searchText;
	public string SearchText
	{
		get => _searchText;
		set
		{
			_searchText = value;
			OnPropertyChanged(nameof(SearchText));
			ApplyFilters();
		}
	}

	public DatabaseViewModel()
	{
		Years = [];
		SelectedYears = [];

		LbMouseLeftButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(OnPreviewMouseLeftButtonDown);
		LbMouseMoveCommand = new RelayCommand<MouseEventArgs>(OnPreviewMouseMove);
		LbMouseLeftButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(OnPreviewMouseLeftButtonUp);

		EditCommand = new RelayCommand(EditTransaction);
		DeleteCommand = new RelayCommand(DeleteTransaction);
		AddNewItemWindowCommand = new RelayCommand(OnOpenAddNewItemWindowCommand);

		_ = LoadDataAsync();

		EventMediator.TransactionAdded += OnTransactionAdded;
	}

	private async Task LoadDataAsync()
	{
		await using var context = new DashboardDbContext();

		var transactions = await context.Transactions.ToListAsync();
		var years = await context.Transactions.Select(m => m.Date.Year.ToString()).Distinct().ToListAsync();

		UpdateUi(years, transactions);
	}

	private void OnTransactionAdded(object sender, EventArgs e)
	{
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
	}

	private async void ApplyFilters()
	{
		if (Transactions == null || SelectedYears == null)
		{
			FilteredTransactions = Transactions;
			return;
		}

		var selectedYears = SelectedYears.Select(index => Years[index]).ToList();
		var searchTextLower = SearchText?.ToLower();
		
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

		ApplyFilters();
	}

	private static void OnOpenAddNewItemWindowCommand(object parameter)
	{
		AddTransactionView addTransactionView = new();
		addTransactionView.Show();
	}

	private void EditTransaction(object parameter)
	{
		if (parameter != null)
		{
			SelectedTransaction = (Transaction)parameter;
			IsEditEnabled = true;
		}

		if (SelectedTransaction == null) return;
		using (var context = new DashboardDbContext())
		{
			var existingTransaction = context.Transactions.Find(SelectedTransaction.Id);
			if (existingTransaction != null)
			{
				existingTransaction.Date = SelectedTransaction.Date;
				existingTransaction.Category = SelectedTransaction.Category;
				existingTransaction.Amount = SelectedTransaction.Amount;
				existingTransaction.Type = SelectedTransaction.Type;

				context.SaveChanges();
			}
		}

		_ = LoadDataAsync();
	}

	private void DeleteTransaction(object parameter)
	{
		if (SelectedTransaction == null) return;
		var confirmation = MessageBox.Show("Вы уверены что хотите удалить выделенную транзакцию?", "Удалить транзакцию", MessageBoxButton.YesNo, MessageBoxImage.Question);

		if (confirmation != MessageBoxResult.Yes) return;
		using (var context = new DashboardDbContext())
		{
			var existingTransaction = context.Transactions.Find(SelectedTransaction.Id);
			if (existingTransaction != null)
			{
				context.Transactions.Remove(existingTransaction);
				context.SaveChanges();
			}
		}

		_ = LoadDataAsync();
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
	}

	private static bool IsItemIntersectsWithSelectionRect(ListBox listBox, int itemIndex, Rect selectionRect)
	{
		if (listBox.ItemContainerGenerator.ContainerFromIndex(itemIndex) is not ListBoxItem listBoxItem) return false;

		var itemPosition = listBoxItem.TranslatePoint(new Point(0, 0), listBox);
		var itemRect = new Rect(itemPosition, new Size(listBoxItem.ActualWidth, listBoxItem.ActualHeight));
		return selectionRect.IntersectsWith(itemRect);
	}
}