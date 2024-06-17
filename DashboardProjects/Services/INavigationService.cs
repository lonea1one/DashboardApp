using DashboardProjects.ViewModels;

namespace DashboardProjects.Services
{
    public interface INavigationService
    {
        BaseViewModel CurrentView { get; }
        BaseViewModel CurrentAdminView { get; }

        void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;
        void NavigateToNested<TViewModel>() where TViewModel : BaseViewModel;
    }
}
