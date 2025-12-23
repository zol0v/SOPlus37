using System.Windows;

namespace SOPlus37.Infrastructure
{
    public class DialogService : IDialogService
    {
        public bool Confirm(string message, string title = "Подтверждение")
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }
    }
}
