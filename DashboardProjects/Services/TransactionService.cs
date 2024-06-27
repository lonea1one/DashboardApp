using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DashboardProjects.Services
{
    public class TransactionService
    {
        private readonly DashboardDbContext _dbContext;

        public TransactionService(DashboardDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            await using var context = new DashboardDbContext();
            return await context.Transactions.ToListAsync();
        }

        public async Task<List<string>> GetDistinctYearsAsync()
        {
            await using var context = new DashboardDbContext();
            return await context.Transactions
                .Select(m => m.Date.Year.ToString())
                .Distinct()
                .ToListAsync();
        }

        public async Task DeleteTransactionAsync(int transactionId)
        {
            await using var context = new DashboardDbContext();
            var transaction = await context.Transactions.FindAsync(transactionId);
            if (transaction != null)
            {
                context.Transactions.Remove(transaction);
                await context.SaveChangesAsync();
            }
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            await using var context = new DashboardDbContext();
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();
        }
    }
}
