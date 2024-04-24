namespace DashboardProjects.Utils
{
    public static class EventMediator
    {
        public static event EventHandler TransactionAdded;

        public static void OnTransactionAdded()
        {
            TransactionAdded?.Invoke(null, EventArgs.Empty);
        }
    }
}