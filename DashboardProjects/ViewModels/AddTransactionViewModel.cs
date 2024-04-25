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
		public ObservableCollection<string> Categories { get; set; }
		public ObservableCollection<string> Types { get; set; }
		public ObservableCollection<string> ExpenseCategories { get; set; }
		public ObservableCollection<string> IncomeCategories { get; set; }
		
		public string AmountErrorMessage { get; set; }
		public string TypeErrorMessage { get; set; }
		public string TransactionErrorMessage { get; set; }
		public string CategoryErrorMessage { get; set; }
		public string DateErrorMessage { get; set; }
		public string SelectedCategory { get; set; }
		public string SelectedType { get; set; }
		public DateTime? SelectedDate { get; set; }
		
		public decimal? Amount { get; set; }
		public string? Category { get; set; }
		
		
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
		
		private Visibility _categoryErrorVisibility = Visibility.Collapsed;
		public Visibility CategoryErrorVisibility
		{
			get => _categoryErrorVisibility;
			set
			{
				_categoryErrorVisibility = value;
				OnPropertyChanged(nameof(CategoryErrorVisibility));
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

		public ICommand AddDataCommand { get; }
		public ICommand CloseWindowCommand { get; }
		public ICommand CategorySelectionChangedCommand { get; }
		public ICommand AmountInputCommand { get; }
		public ICommand CategoryInputCommand { get; }

		private const string SelectCategoryMessage = "Выберите категорию";
		private const string EnterOrSelectCategoryMessage = "Введите или выберите категорию";
		private const string SelectTransactionMessage = "Выберите транзакцию";
		private const string EnterValidAmountMessage = "Введите правильную сумму.";
		private const string PositiveAmountMessage = "Сумма должна быть положительной";
		private const string ExistingCategoryMessage = "Такая категория уже существует";
		private const string DateOutOfRangeMessage = "Выберите дату в период с 2023 по 2024 год";

		[System.Text.RegularExpressions.GeneratedRegex(@"^[0-9.]+$")]
		private static partial System.Text.RegularExpressions.Regex MyRegex();

		public AddTransactionViewModel()
        {
			Categories = [];
			Types = [];
			ExpenseCategories = [];
			IncomeCategories = [];

			SelectedDate = DateTime.Now;

			AddDataCommand = new RelayCommand(OnAddDataCommand);
			CloseWindowCommand = new RelayCommand<Window>(OnCloseWindowCommand);
			CategorySelectionChangedCommand = new RelayCommand(OnCategorySelectionChanged);
			AmountInputCommand = new RelayCommand(OnPreviewAmountInput);
			CategoryInputCommand = new RelayCommand(OnPreviewCategoryInput);

			_ = GetDataAsync();	
		}
		
		public AddTransactionViewModel(Transaction transaction)
		{
			Categories = [];
			Types = [];
			ExpenseCategories = [];
			IncomeCategories = [];

			SelectedDate = transaction.Date;
			SelectedCategory = transaction.Category;
			SelectedType = transaction.Type;
			Amount = transaction.Amount;

			AddDataCommand = new RelayCommand(OnAddDataCommand);
			CloseWindowCommand = new RelayCommand<Window>(OnCloseWindowCommand);
			CategorySelectionChangedCommand = new RelayCommand(OnCategorySelectionChanged);
			AmountInputCommand = new RelayCommand(OnPreviewAmountInput);
			CategoryInputCommand = new RelayCommand(OnPreviewCategoryInput);

			_ = GetDataAsync();	
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

		private void OnCategorySelectionChanged(object parameter)
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
			
			Category = null;
		}

		private static void OnPreviewAmountInput (object parameter)
		{
			var args = parameter as TextCompositionEventArgs;
			if (args?.Source is not TextBox textBox) return;

			if (!(string.IsNullOrEmpty(args.Text) || MyRegex().IsMatch(args.Text)))
			{
				args.Handled = true; 
			}
			else
			{
				if (args.Text == "," && textBox.Text.Contains(','))
				{
					args.Handled = true; 
				}
			}
		}
		
		private void OnPreviewCategoryInput (object parameter)
		{
			var args = parameter as TextCompositionEventArgs;
			if (args?.Source is not TextBox textBox) return;

			if (!char.IsLetter(args.Text, 0))
			{
				args.Handled = true; 
			}
			
			SelectedCategory = null;
		}

		private bool Validate()
		{
			var isValid = true;

			if (string.IsNullOrWhiteSpace(SelectedCategory))
			{
				if (string.IsNullOrWhiteSpace(Category))
				{
					TransactionErrorMessage = SelectCategoryMessage;
					TransactionErrorVisibility = Visibility.Visible;
					
					CategoryErrorMessage = EnterOrSelectCategoryMessage;
					CategoryErrorVisibility = Visibility.Visible;
					
					isValid = false;
				}
				else
				{
					CategoryErrorMessage = string.Empty;
					CategoryErrorVisibility = Visibility.Collapsed;
				}
			}
			else
			{
				CategoryErrorMessage = string.Empty;
				CategoryErrorVisibility = Visibility.Collapsed;

				TransactionErrorMessage = string.Empty;
				TransactionErrorVisibility = Visibility.Collapsed;
			}

			TypeErrorMessage = string.IsNullOrWhiteSpace(SelectedType) ? SelectTransactionMessage : string.Empty;
			TypeErrorVisibility = string.IsNullOrWhiteSpace(SelectedType) ? Visibility.Visible : Visibility.Collapsed;
			isValid &= string.IsNullOrWhiteSpace(TypeErrorMessage);

			DateErrorMessage = SelectedDate == default || SelectedDate < new DateTime(2023, 1, 1) || SelectedDate > DateTime.Now ? DateOutOfRangeMessage : string.Empty;
			DateErrorVisibility = SelectedDate == default || SelectedDate < new DateTime(2023, 1, 1) || SelectedDate > DateTime.Now ? Visibility.Visible : Visibility.Collapsed;
			isValid &= string.IsNullOrWhiteSpace(DateErrorMessage);

			if (string.IsNullOrWhiteSpace(Amount.ToString()))
			{
				AmountErrorMessage = EnterValidAmountMessage;
				isValid = false;
			}
			else
			{
				if (Amount <= 0)
				{
					AmountErrorMessage = PositiveAmountMessage;
					isValid = false;
				}
				else
				{
					AmountErrorMessage = string.Empty;
				}
			}
			AmountErrorVisibility = string.IsNullOrWhiteSpace(AmountErrorMessage) ? Visibility.Collapsed : Visibility.Visible;

			if (!string.IsNullOrWhiteSpace(Category))
			{
				using (var context = new DashboardDbContext())
				{
					var existingCategory = context.Transactions.FirstOrDefault(c => c.Category.Equals(Category, StringComparison.OrdinalIgnoreCase));
					if (existingCategory != null)
					{
						CategoryErrorMessage = ExistingCategoryMessage;
						CategoryErrorVisibility = Visibility.Visible;
						isValid = false;
					}
				}
			}

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
				await using (var context = new DashboardDbContext())
				{
					var transactionCategory = !string.IsNullOrWhiteSpace(SelectedCategory) ? SelectedCategory : Category;
					
					var transaction = new Transaction
					{
						Date = SelectedDate ?? DateTime.Now,
						Amount = Amount ?? 0,
						Category = transactionCategory,
						Type = SelectedType
					};

					await context.Transactions.AddAsync(transaction);
					await context.SaveChangesAsync();
				}
				
				
				EventMediator.OnTransactionAdded();
				MessageBox.Show("Новый данные были успешно добавлены!");
			}
		} 
    }
}
