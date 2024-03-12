using DashboardProjects.ViewModels;
using System.Windows.Controls;

namespace DashboardProjects.Views;

public partial class DatabaseView : UserControl
{
    public DatabaseView()
    {
        InitializeComponent();
        DataContext = new DatabaseViewModel();
    }
}