using DashboardProjects.Commands;
using DashboardProjects.Services;
using System.Windows.Input;

namespace DashboardProjects.ViewModels;

public class MainViewModel : BaseViewModel
{
    public INavigationService Navigation { get; }

	public ICommand WindowKeyDowCommand { get; private set; }
    public RelayCommand NavigateDatabaseCommand { get; }
    public RelayCommand NavigateHomeCommand { get; }

    public ICommand DragMoveCommand { get; }
    public ICommand MouseEnterCommand { get; }
    public ICommand MouseLeaveCommand { get; }

    public ICommand MinimizeCommand { get; }
    public ICommand MaximizeRestoreCommand { get; }
    public ICommand CloseCommand { get; }

	public MainViewModel(INavigationService navigationService)
    {
        WindowKeyDowCommand = new RelayCommand<KeyEventArgs>(OnWindowKeyDow);
        DragMoveCommand = new DragMoveCommand();
        MouseEnterCommand = new RelayCommand(OnMouseEnter);
        MouseLeaveCommand = new RelayCommand(OnMouseLeave);
        NavigateDatabaseCommand = new RelayCommand(o => { Navigation.NavigateTo<DatabaseViewModel>(); }, o => true);
        NavigateHomeCommand = new RelayCommand(o => { Navigation.NavigateTo<HomeViewModel>(); }, o => true);

        MinimizeCommand = new MinimizeCommand();
        MaximizeRestoreCommand = new MaximizeRestoreCommand();
        CloseCommand = new CloseCommand();

        Navigation = navigationService;
        Navigation.NavigateTo<HomeViewModel>();
    }

	private static void OnWindowKeyDow(object parameter)
	{
		if (parameter is not KeyEventArgs e) return;

		if (e.Key == Key.Escape || e is { Key: Key.System, SystemKey: Key.LeftAlt } || e is { Key: Key.System, SystemKey: Key.RightAlt })
		{
			e.Handled = true;
		}
	}

    private void OnMouseEnter(object parameter)
    {
        Mouse.OverrideCursor = Cursors.Hand;
    }

    private void OnMouseLeave(object parameter)
    {
        Mouse.OverrideCursor = Cursors.Arrow;
    }
}