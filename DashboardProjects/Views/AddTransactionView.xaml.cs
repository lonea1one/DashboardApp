using DashboardProjects.ViewModels;
using DataAccess.Models;

namespace DashboardProjects.Views;

public partial class AddTransactionView
{
    public AddTransactionView()
    {
        InitializeComponent();
    }
    
    public AddTransactionView(Transaction transaction)
    {
        InitializeComponent();
        DataContext = new AddTransactionViewModel(transaction);
    }
}