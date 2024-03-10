using System.Windows;
using DashboardProjects.ViewModels;

namespace DashboardProjects.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}