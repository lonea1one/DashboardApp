using System.Windows;
using System.Windows.Input;

namespace DashboardProjects.Commands;

public class MaximizeRestoreCommand : ICommand
{
    private bool _isMaximized = false;
    
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        if (parameter is not RoutedEventArgs args || args.OriginalSource is not FrameworkElement element) return;
        var window = Window.GetWindow(element);

        if (window == null) return;
        if (!_isMaximized)
        {
            window.WindowState = WindowState.Maximized;
            _isMaximized = true;
        }
        else
        {
            window.WindowState = WindowState.Normal;
            _isMaximized = false;
        }
    }
}
