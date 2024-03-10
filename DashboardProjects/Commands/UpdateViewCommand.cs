using System.Windows.Input;
using DashboardProjects.ViewModels;

namespace DashboardProjects.Commands;

public class UpdateViewCommand : ICommand
{
    private MainViewModel _mainViewModel;
    
    public UpdateViewCommand(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }
    
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        switch (parameter.ToString())
        {
            case "Home":
                _mainViewModel.SelectedViewModel = new HomeViewModel();
                break;
            case "Database":
                _mainViewModel.SelectedViewModel = new DatabaseViewModel();
                break;
            case "Settings":
                _mainViewModel.SelectedViewModel = new SettingsViewModel();
                break;
            case "Bookmarks":
                _mainViewModel.SelectedViewModel = new BookmarksViewModel();
                break;
        }
    }
}