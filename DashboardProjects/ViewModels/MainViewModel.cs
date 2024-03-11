using DashboardProjects.Commands;
using System.Windows.Input;

namespace DashboardProjects.ViewModels;

public class MainViewModel : BaseViewModel
{
    private BaseViewModel _selectedViewModel;

    public BaseViewModel SelectedViewModel
    {
        get => _selectedViewModel;
        set
        {
            _selectedViewModel = value;
            OnPropertyChanged(nameof(SelectedViewModel));
        }
    }

	public ICommand WindowKeyDowCommand { get; private set; }

	public ICommand UpdateViewCommand { get; }

    public ICommand DragMoveCommand { get; }

    public ICommand MinimizeCommand { get; }
    public ICommand MaximizeRestoreCommand { get; }
    public ICommand CloseCommand { get; }

	public MainViewModel()
    {
		WindowKeyDowCommand = new RelayCommand<KeyEventArgs>(OnWindowKeyDow);

		UpdateViewCommand = new UpdateViewCommand(this);
        
        DragMoveCommand = new DragMoveCommand();
        
        MinimizeCommand = new MinimizeCommand();
        MaximizeRestoreCommand = new MaximizeRestoreCommand();
        CloseCommand = new CloseCommand();
    }

	private static void OnWindowKeyDow(object parameter)
	{
		if (parameter is not KeyEventArgs e) return;

		if (e.Key == Key.Escape || e is { Key: Key.System, SystemKey: Key.LeftAlt } || e is { Key: Key.System, SystemKey: Key.RightAlt })
		{
			// Отмена действия по умолчанию
			e.Handled = true;
		}
	}
}