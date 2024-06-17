using DashboardProjects.Utils;
using DashboardProjects.ViewModels;

namespace DashboardProjects.Services
{
    public class NavigationService : ObservableObject, INavigationService
    {
        private Func<Type, BaseViewModel> _viewModelFactory { get; }

        private BaseViewModel _currentView;
        public BaseViewModel CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        private BaseViewModel _currentAdminView;
        public BaseViewModel CurrentAdminView
        {
            get => _currentAdminView;
            private set
            {
                _currentAdminView = value;
                OnPropertyChanged();
            }
        }

        public NavigationService(Func<Type, BaseViewModel> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            var viewModel = _viewModelFactory.Invoke(typeof(TViewModel));
            CurrentView = viewModel;
        }

        public void NavigateToNested<TViewModel>() where TViewModel : BaseViewModel
        {
            var viewModel = _viewModelFactory.Invoke(typeof(TViewModel));
            CurrentAdminView = viewModel;
        }
    }
}
