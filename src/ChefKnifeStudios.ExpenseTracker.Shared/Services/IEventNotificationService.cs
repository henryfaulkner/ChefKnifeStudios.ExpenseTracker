namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public delegate Task EventReceivedEventHandler(
    object sender, IEventArgs e);

public interface IEventNotificationService
{
    event EventReceivedEventHandler? EventReceived;
    void PostEvent(object sender, IEventArgs args);
}

public interface IEventArgs
{
}