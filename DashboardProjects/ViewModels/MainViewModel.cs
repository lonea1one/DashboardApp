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
    
    public ICommand UpdateViewCommand { get; }
    
    public MainViewModel()
    {
        UpdateViewCommand = new UpdateViewCommand(this);
    }
}