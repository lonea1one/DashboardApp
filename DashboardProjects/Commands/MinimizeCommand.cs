using System.Windows;
using System.Windows.Input;

namespace DashboardProjects.Commands;

public class MinimizeCommand : ICommand
{
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        if (parameter is not RoutedEventArgs args || args.OriginalSource is not FrameworkElement element) return;
        var window = Window.GetWindow(element);

        if (window != null)
        {
            window.WindowState = WindowState.Minimized;
        }
    }
}