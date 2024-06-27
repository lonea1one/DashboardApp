using DataAccess;
using Serilog;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DashboardProjects.Views;
using DashboardProjects.ViewModels;
using Microsoft.Extensions.Logging;
using DashboardProjects.Services;

namespace DashboardProjects
{
    public partial class App
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .WriteTo.Console()
               .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();

            // Настройка встроенного логгера
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddSerilog();
        }

        private void ConfigureServices(IServiceCollection services)
        {
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

            services.AddScoped<DataInitializationService>();
            services.AddScoped<DataService>();
            services.AddScoped<FileProcessingService>();
            services.AddScoped<TransactionService>();
            services.AddScoped<ExcelDataReaderService>();

            services.AddSingleton<Func<Type, BaseViewModel>>(serviceProvider => viewModelType => (BaseViewModel)serviceProvider.GetRequiredService(viewModelType));
            services.AddSingleton<INavigationService, NavigationService>();

            // Добавление логгера
            services.AddLogging(configure => configure.AddSerilog());
        }

        protected override void OnStartup(StartupEventArgs e)
		{
            var logger = _serviceProvider.GetRequiredService<ILogger<App>>();
            try
			{
				Task.Run(async () =>
				{
                    var dbContext = _serviceProvider.GetRequiredService<DashboardDbContext>();
                    var dataInitializationService = _serviceProvider.GetRequiredService<DataInitializationService>();

                    await dbContext.Database.EnsureCreatedAsync();
					await dataInitializationService.SeedDataAsync();
				}).GetAwaiter().GetResult();

                MainWindow = _serviceProvider.GetRequiredService<MainView>();
                MainWindow.Show();
                base.OnStartup(e);

                logger.LogInformation("Приложение успешно запущено.");
            }
			catch (Exception ex)
			{
                logger.LogCritical(ex, "Не удалось инициализировать базу данных.");
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				Current.Shutdown();
			}
		}
	}

}
