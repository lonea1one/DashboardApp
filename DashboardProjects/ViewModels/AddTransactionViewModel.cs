using DashboardProjects.Commands;
using DashboardProjects.Utils;
using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DashboardProjects.ViewModels
{
    public partial class AddTransactionViewModel : BaseViewModel
    {
		private ObservableCollection<string> _categories;
		public ObservableCollection<string> Categories
		{
			get => _categories;
			set
			{
				_categories = value;
				OnPropertyChanged(nameof(Categories));
			}
		}

		private ObservableCollection<string> _types;
		public ObservableCollection<string> Types
		{
			get => _types;
			set
			{
				_types = value;
				OnPropertyChanged(nameof(Types));
			}
		}

		private ObservableCollection<string> _expenseCategories;
		public ObservableCollection<string> ExpenseCategories
		{
			get => _expenseCategories;
			set
			{
				_expenseCategories = value;
				OnPropertyChanged(nameof(ExpenseCategories));
			}
		}

		private ObservableCollection<string> _incomeCategories;
		public ObservableCollection<string> IncomeCategories
		{
			get => _incomeCategories;
			set
			{
				_incomeCategories = value;
				OnPropertyChanged(nameof(IncomeCategories));
			}
		}

		private Visibility _dateErrorVisibility = Visibility.Collapsed;
		public Visibility DateErrorVisibility
		{
			get => _dateErrorVisibility;
			set
			{
				_dateErrorVisibility = value;
				OnPropertyChanged(nameof(DateErrorVisibility));
			}
		}

		private Visibility _transactionErrorVisibility = Visibility.Collapsed;
		public Visibility TransactionErrorVisibility
		{
			get => _transactionErrorVisibility;
			set
			{
				_transactionErrorVisibility = value;
				OnPropertyChanged(nameof(TransactionErrorVisibility));
			}
		}

		private Visibility _typeErrorVisibility = Visibility.Collapsed;
		public Visibility TypeErrorVisibility
		{
			get => _typeErrorVisibility;
			set
			{
				_typeErrorVisibility = value;
				OnPropertyChanged(nameof(TypeErrorVisibility));
			}
		}

		private Visibility _amountErrorVisibility = Visibility.Collapsed;
		public Visibility AmountErrorVisibility
		{
			get => _amountErrorVisibility;
			set
			{
				_amountErrorVisibility = value;
				OnPropertyChanged(nameof(AmountErrorVisibility));
			}
		}

		private string _amountErrorMessage;
		public string AmountErrorMessage
		{
			get => _amountErrorMessage;
			set
			{
				_amountErrorMessage = value;
				OnPropertyChanged(nameof(AmountErrorMessage));
			}
		}

		private string _typeErrorMessage;
		public string TypeErrorMessage
		{
			get => _typeErrorMessage;
			set
			{
				_typeErrorMessage = value;
				OnPropertyChanged(nameof(TypeErrorMessage));
			}
		}

		private string _transactionErrorMessage;
		public string TransactionErrorMessage
		{
			get => _transactionErrorMessage;
			set
			{
				_transactionErrorMessage = value;
				OnPropertyChanged(nameof(TransactionErrorMessage));
			}
		}

		private string _dateErrorMessage;
		public string DateErrorMessage
		{
			get => _dateErrorMessage;
			set
			{
				_dateErrorMessage = value;
				OnPropertyChanged(nameof(DateErrorMessage));
			}
		}

		private string _selectedCategory;
		public string SelectedCategory
		{
			get => _selectedCategory;
			set
			{
				_selectedCategory = value;
				OnPropertyChanged(nameof(SelectedCategory));
			}
		}

		private string _selectedType;
		public string SelectedType
		{
			get => _selectedType;
			set
			{
				_selectedType = value;
				OnPropertyChanged(nameof(SelectedType));
			}
		}

		private DateTime? _selectedDate;
		public DateTime? SelectedDate
		{
			get => _selectedDate;
			set
			{
				_selectedDate = value;
				OnPropertyChanged(nameof(SelectedDate));
			}
		}


		private decimal? _amount;
		public decimal? Amount
		{
			get => _amount;
			set
			{
				_amount = value;
				OnPropertyChanged(nameof(Amount));
			}
		}

		public ICommand AddDataCommand { get; }
		public ICommand CloseWindowCommand { get; }
		public ICommand CategorySelectionChangedCommand { get; }
		public ICommand TextInputCommand { get; }

		public AddTransactionViewModel()
        {
			Categories = [];
			Types = [];
			ExpenseCategories = [];
			IncomeCategories = [];

			SelectedDate = DateTime.Now;

			AddDataCommand = new RelayCommand(OnAddDataCommand);
			CloseWindowCommand = new RelayCommand<Window>(OnCloseWindowCommand);
			CategorySelectionChangedCommand = new RelayCommand(OnTransactionSelectionChanged);
			TextInputCommand = new RelayCommand(OnPreviewTextInput);

			Task.Run(GetDataAsync);	
		}

		private void OnAddDataCommand(object parameter)
		{
			_ = AddDataAsync();
		}

		private static void OnCloseWindowCommand(Window window)
		{
			if (window != null)
			{
				window.Close();
			}
		}

		private void OnTransactionSelectionChanged(object parameter)
		{
			var args = parameter as SelectionChangedEventArgs;
			if (args?.Source is not ComboBox comboBox) return;

			if (comboBox.SelectedItem is not string selectedTransaction) return;
			if (ExpenseCategories.Contains(selectedTransaction))
			{
				SelectedType = "РАСХОДЫ";
			}
			else if (IncomeCategories.Contains(selectedTransaction))
			{
				SelectedType = "ДОХОДЫ";
			}
		}

		private static void OnPreviewTextInput (object parameter)
		{
			var args = parameter as TextCompositionEventArgs;
			if (args?.Source is not TextBox textBox) return;

			if (!(string.IsNullOrEmpty(args.Text) || MyRegex().IsMatch(args.Text)))
			{
				args.Handled = true; // Если нет, отменяем ввод
			}
			else
			{
				// Проверяем, содержит ли новый текст только одну запятую
				if (args.Text == "," && textBox.Text.Contains(','))
				{
					args.Handled = true; // Если содержит, отменяем ввод
				}
			}
		}

		private bool Validate()
		{
			var isValid = true;

			TransactionErrorMessage = string.IsNullOrWhiteSpace(SelectedCategory) ? "Выберите категорию" : string.Empty;
			TransactionErrorVisibility = string.IsNullOrWhiteSpace(SelectedCategory) ? Visibility.Visible : Visibility.Collapsed;
			isValid &= string.IsNullOrWhiteSpace(TransactionErrorMessage);

			TypeErrorMessage = string.IsNullOrWhiteSpace(SelectedType) ? "Выберите транзакцию" : string.Empty;
			TypeErrorVisibility = string.IsNullOrWhiteSpace(SelectedType) ? Visibility.Visible : Visibility.Collapsed;
			isValid &= string.IsNullOrWhiteSpace(TypeErrorMessage);

			DateErrorMessage = SelectedDate == default || SelectedDate < new DateTime(DateTime.Now.Year, 1, 1) || SelectedDate > DateTime.Now ? "Выберите дату в текущем году" : string.Empty;
			DateErrorVisibility = SelectedDate == default || SelectedDate < new DateTime(DateTime.Now.Year, 1, 1) || SelectedDate > DateTime.Now ? Visibility.Visible : Visibility.Collapsed;
			isValid &= string.IsNullOrWhiteSpace(DateErrorMessage);

			isValid &= decimal.TryParse(Amount.ToString(), out var amount) && amount > 0;
			AmountErrorMessage = isValid ? string.Empty : (string.IsNullOrWhiteSpace(Amount.ToString()) ? "Введите правильную сумму." : (amount == 0) ? "Сумма должна быть больше 0"  : "Сумма должна быть положительной");
			AmountErrorVisibility = isValid ? Visibility.Collapsed : Visibility.Visible;

			return isValid;
		}

		private async Task GetDataAsync()
        {
			await using var context = new DashboardDbContext();
			var categories = await context.Transactions.Select(x => x.Category.ToString()).Distinct().ToListAsync();
			var types = await context.Transactions.Select(x => x.Type.ToString()).Distinct().ToListAsync();

			var expenses = await context.Transactions.Where(x => x.Type == "РАСХОДЫ").Select(m => m.Category).Distinct().ToListAsync();
			var incomes = await context.Transactions.Where(x => x.Type == "ДОХОДЫ").Select(m => m.Category).Distinct().ToListAsync();

			Categories = new ObservableCollection<string>(categories);
			Types = new ObservableCollection<string>(types);
			
			ExpenseCategories = new ObservableCollection<string>(expenses);
			IncomeCategories = new ObservableCollection<string>(incomes);
		}

		private async Task AddDataAsync()
		{
			var isValidData = Validate();

			if (isValidData)
			{
				await using var context = new DashboardDbContext();
				var transaction = new Transaction
				{
					Date = SelectedDate ?? DateTime.Now,
					Amount = Amount ?? 0,
					Category = SelectedCategory,
					Type = SelectedType
				};

				await context.Transactions.AddAsync(transaction);
				await context.SaveChangesAsync();

				EventMediator.OnTransactionAdded();
				MessageBox.Show("Новый данные были успешно добавлены!");
			}
		}

        [System.Text.RegularExpressions.GeneratedRegex(@"^[0-9.]+$")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
    }
}
