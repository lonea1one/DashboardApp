using DataAccess;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DashboardProjects.Views;
using DashboardProjects.ViewModels;
using DashboardProjects.Services;
using Microsoft.Extensions.Logging;

namespace DashboardProjects
{
    public partial class App
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton(provider => new MainView
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });

            services.AddDbContext<DashboardDbContext>();

            services.AddSingleton<MainViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<DatabaseViewModel>();
            services.AddTransient<BookmarksViewModel>();
            services.AddTransient<AddTransactionViewModel>();

            services.AddSingleton<Func<Type, BaseViewModel>>(serviceProvider => viewModelType => (BaseViewModel)serviceProvider.GetRequiredService(viewModelType));
            services.AddSingleton<INavigationService, NavigationService>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
		{
			try
			{
				Task.Run(async () =>
				{
                    await using var dbContext = new DashboardDbContext();
                    await dbContext.Database.EnsureCreatedAsync();
					await dbContext.SeedDataAsync();
				}).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
                // Логирование ошибки
                var logger = _serviceProvider.GetRequiredService<ILogger<App>>();
                logger.LogError(ex, "Не удалось инициализировать базу данных.");
                // Показать сообщение пользователю
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				// Завершить работу приложения
				Current.Shutdown();
			}
		}
	}

}
