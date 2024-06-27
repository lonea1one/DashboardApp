using DataAccess.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DashboardProjects.Services
{
    public class DataService
    {
        private readonly DashboardDbContext _dbContext;

        public DataService(DashboardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<string>> GetExpenseCategoriesAsync()
        {
            await using var context = new DashboardDbContext();
            return await context.Transactions
                .Where(x => x.Type == "РАСХОДЫ")
                .Select(m => m.Category)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetIncomeCategoriesAsync()
        {
            await using var context = new DashboardDbContext();
            return await context.Transactions
                .Where(x => x.Type == "ДОХОДЫ")
                .Select(m => m.Category)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetYearsAsync()
        {
            await using var context = new DashboardDbContext();
            return await context.Transactions
                .Select(m => m.Date.Year.ToString())
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            await using var context = new DashboardDbContext();
            return await context.Transactions.ToListAsync();
        }

        public async Task<List<(string Category, decimal Sum)>> GetCategorySumAsync(string type)
        {
            await using var context = new DashboardDbContext();

            var categories = await context.Transactions
                .Where(x => x.Type == type)
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Sum = g.Sum(t => (double)t.Amount) })
                .ToListAsync();

            return categories.Select(item => (item.Category, (decimal)item.Sum)).ToList();
        }

        public async Task<List<(DateTime Month, decimal TotalAmount, decimal Percentage)>> FetchChartDataAsync(string type, List<string> selectedCategories, List<string> selectedYears)
        {
            await using var context = new DashboardDbContext();

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

        public async Task<decimal> FetchBalanceForCurrentMonthAsync()
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

        public async Task<List<(string MonthName, decimal Balance)>> FetchBalanceChartDataAsync(List<string> selectedYears)
        {
            await using var context = new DashboardDbContext();

            var balances = new List<Balance>();

            var now = DateTime.Now;

            var isPreviousYearSelected = selectedYears.Any(year => year == (now.Year - 1).ToString());
            var isCurrentYearSelected = selectedYears.Any(year => year == now.Year.ToString());

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
                foreach (var year in selectedYears)
                {
                    if (year == now.Year.ToString())
                    {
                        var startDate = now.AddYears(-1);

                        balances.AddRange(await context.Balances
                            .Where(b =>
                                (b.Year > startDate.Year || (b.Year == startDate.Year && b.Month >= startDate.Month)) &&
                                (b.Year < now.Year || (b.Year == now.Year && b.Month <= now.Month)))
                            .ToListAsync());
                    }
                    else
                    {
                        balances.AddRange(await context.Balances
                            .Where(b => b.Year.ToString() == year)
                            .ToListAsync());
                    }
                }
            }

            var balanceChartData = balances
                .OrderBy(b => b.Year).ThenBy(b => b.Month)
                .Select(b => (Month: b.MonthName, Balance: b.BalanceAmount))
                .ToList();

            return balanceChartData;
        }
    }
}
