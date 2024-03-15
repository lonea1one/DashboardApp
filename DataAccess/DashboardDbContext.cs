using ClosedXML.Excel;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess;
public sealed class DashboardDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Balance> Balances { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
	    var config = new ConfigurationBuilder()
		    .AddJsonFile("appsettings.json")
		    .SetBasePath(Directory.GetCurrentDirectory())
		    .Build();
	    
        optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
	    modelBuilder.Entity<Transaction>()
		    .HasIndex(t => t.Date);

	    modelBuilder.Entity<Transaction>()
		    .HasIndex(t => t.Type); 

	    modelBuilder.Entity<Transaction>()
		    .HasIndex(t => t.Amount); 

	    modelBuilder.Entity<Transaction>()
		    .HasIndex(t => t.Category); 

		modelBuilder.Entity<Balance>()
			.HasIndex(t => t.Month); 

		modelBuilder.Entity<Balance>()
			.HasIndex(t => t.Year); 

		modelBuilder.Entity<Balance>()
			.HasIndex(t => t.BalanceAmount); 

	}

	public async Task SeedDataAsync()
	{
		if (await Database.CanConnectAsync())
		{
			if (!(await Transactions.AnyAsync())) 
			{
				var data = await ReadExcelDataAsync(@"Files/dashboard.xlsx");
				Transactions.AddRange(data); 
				await SaveChangesAsync(); 
				await UpdateBalancesAsync();
			}
		}
	}

	private static async Task<List<Transaction>> ReadExcelDataAsync(string filePath)
	{
		return await Task.Run(() =>
		{
			var data = new List<Transaction>();

			using var workbook = new XLWorkbook(filePath);
			var worksheet = workbook.Worksheet(1);
			var rows = worksheet.RangeUsed().RowsUsed();

			var isFirstRow = true;

			foreach (var row in rows)
			{
				if (isFirstRow)
				{
					isFirstRow = false;
					continue;
				}

				var model = new Transaction
				{
					Date = row.Cell(1).GetDateTime(),
					Type = row.Cell(2).GetValue<string>(),
					Amount = (decimal)row.Cell(3).GetDouble(),
					Category = row.Cell(4).GetValue<string>()
				};

				data.Add(model);
			}

			return data;
		});
	}

	public async Task ReadAndSaveExcelData(string filePath)
	{
		var newData = await ReadExcelDataAsync(filePath);

		await using var context = new DashboardDbContext();

		// Очистка существующих данных
		context.Transactions.RemoveRange(context.Transactions);
		context.Balances.RemoveRange(context.Balances);

		// Сохранение изменений
		await context.SaveChangesAsync();

		// Добавление новых данных
		context.Transactions.AddRange(newData);

		// Пересчитываем балансы
		await RecalculateBalancesAsync(context, newData);

		// Сохранение всех изменений, включая новые транзакции и балансы
		await context.SaveChangesAsync();
	}

	private static async Task RecalculateBalancesAsync(DashboardDbContext context, IReadOnlyCollection<Transaction> transactions)
	{
		// Сброс баланса к начальному значению
		var initialBalance = 180305.19m; // Начальный баланс
		var currentDate = new DateTime(2022, 12, 1); // Дата начала
		var maxDate = transactions.Max(t => t.Date);

		while (currentDate <= maxDate)
		{
			var nextMonth = currentDate.AddMonths(1);
			var monthlyTransactions = transactions
				.Where(t => t.Date >= currentDate && t.Date < nextMonth).ToList();

			var monthlyIncome = monthlyTransactions
				.Where(t => t.Type == "ДОХОДЫ").Sum(t => t.Amount);

			var monthlyExpenses = monthlyTransactions
				.Where(t => t.Type == "РАСХОДЫ").Sum(t => t.Amount);

			// Рассчитываем баланс за месяц
			initialBalance += monthlyIncome - monthlyExpenses;

			// Добавляем или обновляем запись баланса
			var balance = new Balance
			{
				Year = currentDate.Year,
				Month = currentDate.Month,
				BalanceAmount = initialBalance
			};
			await context.Balances.AddAsync(balance);

			currentDate = nextMonth;
		}
	}

	private static async Task UpdateBalancesAsync()
	{
		var initialBalance = 180305.19m; // Начальный баланс за декабрь 2022
		var currentDate = new DateTime(2022, 12, 1); // Начинаем с декабря 2022

		await using var context = new DashboardDbContext();
		var maxDate = context.Transactions.Max(t => t.Date); // Находим последнюю дату в транзакциях

		while (currentDate <= maxDate)
		{
			var nextMonth = currentDate.AddMonths(1);

			// Суммируем доходы и расходы за месяц
			var monthlyIncome = context.Transactions
				.Where(t => t.Date >= currentDate && t.Date < nextMonth && t.Type == "ДОХОДЫ")
				.Sum(t => t.Amount);

			var monthlyExpenses = context.Transactions
				.Where(t => t.Date >= currentDate && t.Date < nextMonth && t.Type == "РАСХОДЫ")
				.Sum(t => t.Amount);

			// Рассчитываем баланс за месяц
			initialBalance += monthlyIncome - monthlyExpenses;

			// Проверяем, существует ли уже запись баланса за этот месяц
			var balance = context.Balances
				.FirstOrDefault(b => b.Year == currentDate.Year && b.Month == currentDate.Month);

			if (balance != null)
			{
				// Если запись существует, обновляем её
				balance.BalanceAmount = initialBalance;
			}
			else
			{
				// Если записи нет, создаём новую
				await context.Balances.AddAsync(new Balance
				{
					Year = currentDate.Year,
					Month = currentDate.Month,
					BalanceAmount = initialBalance
				});
			}

			// Переходим к следующему месяцу
			currentDate = nextMonth;
		}

		await context.SaveChangesAsync(); // Сохраняем изменения в базе данных
	}
}
