namespace SOPlus37.Infrastructure
{
    public interface IDialogService
    {
        bool Confirm(string message, string title = "Подтверждение");
    }
}
