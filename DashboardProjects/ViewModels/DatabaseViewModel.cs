using DashboardProjects.Commands;
using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DashboardProjects.ViewModels;

public class DatabaseViewModel : BaseViewModel
{
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

	public ICommand LbMouseLeftButtonDownCommand { get; private set; }
	public ICommand LbMouseMoveCommand { get; private set; }
	public ICommand LbMouseLeftButtonUpCommand { get; private set; }

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

	public DatabaseViewModel()
	{
		Years = [];
		SelectedYears = [];

		LbMouseLeftButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(OnPreviewMouseLeftButtonDown);
		LbMouseMoveCommand = new RelayCommand<MouseEventArgs>(OnPreviewMouseMove);
		LbMouseLeftButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(OnPreviewMouseLeftButtonUp);

		_ = LoadDataAsync();
	}

	private async Task LoadDataAsync()
	{
		await using var context = new DashboardDbContext();

		var transactions = await context.Transactions.ToListAsync();
		var years = await context.Transactions.Select(m => m.Date.Year.ToString()).Distinct().ToListAsync();

		// Обновление UI происходит в потоке UI
		Application.Current.Dispatcher.Invoke(() => UpdateUi(years, transactions));
	}

	private void UpdateUi(List<string> years, List<Transaction> transactions)
	{
		Years = new ObservableCollection<string>(years);
		Transactions = new ObservableCollection<Transaction>(transactions);
		InitializeSelectedItems();
	}

	private void InitializeSelectedItems()
	{
		SelectedYears?.Clear();

		for (var i = 0; i < Years?.Count; i++) SelectedYears?.Add(i);
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

	}

	private void UpdateSelection(ListBox listBox, Rect selectionRect, bool isSimpleClick = false)
	{
		var targetCollection = SelectedYears;

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
}