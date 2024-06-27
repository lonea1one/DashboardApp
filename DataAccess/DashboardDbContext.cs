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
}
