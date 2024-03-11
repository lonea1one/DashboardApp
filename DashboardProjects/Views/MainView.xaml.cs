using System.Windows;
using DashboardProjects.ViewModels;

namespace DashboardProjects.Views;

public partial class MainView
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}