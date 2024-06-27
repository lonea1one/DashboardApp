using DataAccess.Models;
using DataAccess;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DashboardProjects.Services
{
    public class DataInitializationService
    {
        private readonly DashboardDbContext _context;
        private readonly ExcelDataReaderService _excelDataReaderService;
        private readonly ILogger<DataInitializationService> _logger;

        public DataInitializationService(DashboardDbContext context, ExcelDataReaderService excelDataReaderService, ILogger<DataInitializationService> logger)
        {
            _context = context;
            _excelDataReaderService = excelDataReaderService;
            _logger = logger;
        }

        public async Task SeedDataAsync()
        {
            _logger.LogInformation("Начало инициализации данных.");
            await using var context = new DashboardDbContext();
            try
            {
                if (await context.Database.CanConnectAsync())
                {
                    _logger.LogInformation("Успешное подключение к базе данных.");
                    if (!await context.Transactions.AnyAsync())
                    {
                        _logger.LogInformation("База данных пуста. Начинаем загрузку данных из Excel.");
                        var data = await _excelDataReaderService.ReadExcelDataAsync(@"Files/dashboard.xlsx");
                        context.Transactions.AddRange(data);
                        await context.SaveChangesAsync();
                        await UpdateBalancesAsync();
                        _logger.LogInformation("Данные успешно загружены и сохранены в базу данных.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критическая ошибка при инициализации данных. Приложение не может продолжать работу.");
            }
        }

        public async Task ReadAndSaveExcelData(string filePath)
        {
            _logger.LogInformation($"Начало чтения и сохранения данных из файла {filePath}.");

            try
            {
                await using var context = new DashboardDbContext();
                var newData = await _excelDataReaderService.ReadExcelDataAsync(filePath);

                // Очистка существующих данных
                context.Transactions.RemoveRange(context.Transactions);
                context.Balances.RemoveRange(context.Balances);

                // Сохранение изменений
                await context.SaveChangesAsync();

                // Добавление новых данных
                context.Transactions.AddRange(newData);

                // Пересчитываем балансы
                await RecalculateBalancesAsync(newData);

                // Сохранение всех изменений, включая новые транзакции и балансы
                await context.SaveChangesAsync();
                _logger.LogInformation("Данные успешно обновлены в базе данных.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при чтении и сохранении данных из Excel файла.");
            }
        }

        private async Task RecalculateBalancesAsync(IReadOnlyCollection<Transaction> transactions)
        {
            _logger.LogInformation("Начало пересчета балансов.");

            try
            {
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

                    await using var context = new DashboardDbContext();
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

                _logger.LogInformation("Балансы успешно пересчитаны.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при пересчете балансов.");
            }
        }

        public async Task UpdateBalancesAsync()
        {
            _logger.LogInformation("Начало обновления балансов.");

            try
            {
                await using var context = new DashboardDbContext();
                var initialBalance = 180305.19m; // Начальный баланс за декабрь 2022
                var currentDate = new DateTime(2022, 12, 1); // Начинаем с декабря 2022

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

                _logger.LogInformation("Балансы успешно обновлены.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении балансов.");
            }
        }
    }
}
