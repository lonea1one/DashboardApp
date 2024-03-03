using System.Windows;
using DashboardProjects.ViewModels;

namespace DashboardProjects.Views
{
    public partial class TransactionWindow : Window
    {
		public TransactionWindow()
        {
            InitializeComponent();
			DataContext = new TransactionViewModel();
		}
	}
}