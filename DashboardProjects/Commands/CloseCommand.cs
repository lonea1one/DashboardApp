using System.Windows.Input;

namespace DashboardProjects.Commands;

public class CloseCommand : ICommand
{
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        System.Windows.Application.Current.Shutdown();
    }
}