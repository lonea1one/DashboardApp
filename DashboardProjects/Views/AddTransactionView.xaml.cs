using DashboardProjects.ViewModels;
using DataAccess.Models;
using Microsoft.Extensions.Logging;

namespace DashboardProjects.Views;

public partial class AddTransactionView
{
    private readonly ILogger<DatabaseViewModel> _logger;

    public AddTransactionView(ILogger<DatabaseViewModel> logger)
    {
        InitializeComponent();
        _logger = logger;
        DataContext = new AddTransactionViewModel(logger);
    }

    public AddTransactionView(Transaction transaction, ILogger<DatabaseViewModel> logger)
    {
        InitializeComponent();
        _logger = logger;
        DataContext = new AddTransactionViewModel(transaction, logger);
    }
}