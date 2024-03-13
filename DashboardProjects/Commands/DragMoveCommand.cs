using System.Windows;
using System.Windows.Input;

namespace DashboardProjects.Commands;

public class DragMoveCommand : ICommand
{
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        if (parameter is not MouseButtonEventArgs args || args.OriginalSource is not FrameworkElement element) return;

		if (args.ChangedButton != MouseButton.Left)
			return;

		var window = Window.GetWindow(element);

        window?.DragMove();
    }
}