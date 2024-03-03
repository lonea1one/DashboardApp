using DataAccess;
using System.Diagnostics;
using System.Windows;

namespace DashboardProjects
{
    public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
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
				Debug.WriteLine("Не удалось инициализировать базу данных: " + ex.Message);
				// Показать сообщение пользователю
				MessageBox.Show("Произошла ошибка при инициализации базы данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				// Завершить работу приложения
				Current.Shutdown();
			}
		}
	}

}
