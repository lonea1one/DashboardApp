using DashboardProjects.ViewModels;

namespace DashboardProjects.Views;

public partial class HomeView
{
    public HomeView()
    {
        InitializeComponent();
        DataContext = new HomeViewModel();
    }
}