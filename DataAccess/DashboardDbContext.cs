using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;

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

        optionsBuilder.UseSqlServer(config.GetConnectionString("SQLServerWorkConnection"));
    }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		var transaction = modelBuilder.Entity<Transaction>();
		var balance = modelBuilder.Entity<Balance>();

		transaction.HasIndex(t => t.Date);
		transaction.HasIndex(t => t.Type);
		transaction.HasIndex(t => t.Amount);
		transaction.HasIndex(t => t.Category);
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
        var data = new List<Transaction>();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows;

        for (var row = 2; row <= rowCount; row++)
        {
            var transaction = new Transaction
            {
                Date = worksheet.Cells[row, 1].GetValue<DateTime>(),
                Type = worksheet.Cells[row, 2].GetValue<string>(),
                Amount = worksheet.Cells[row, 3].GetValue<decimal>(),
                Category = worksheet.Cells[row, 4].GetValue<string>()
            };

            data.Add(transaction);
        }

        return await Task.FromResult(data);
    }

    public static async Task ReadAndSaveExcelData(string filePath)
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

            var monthlyIncome = (decimal)monthlyTransactions
                .Where(t => t.Type == "ДОХОДЫ").Sum(t => (double)t.Amount);

            var monthlyExpenses = (decimal)monthlyTransactions
                .Where(t => t.Type == "РАСХОДЫ").Sum(t => (double)t.Amount);

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
            var monthlyIncome = (decimal)context.Transactions
                .Where(t => t.Date >= currentDate && t.Date < nextMonth && t.Type == "ДОХОДЫ")
                .Sum(t => (double)t.Amount);

            var monthlyExpenses = (decimal)context.Transactions
                .Where(t => t.Date >= currentDate && t.Date < nextMonth && t.Type == "РАСХОДЫ")
                .Sum(t => (double)t.Amount);

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
