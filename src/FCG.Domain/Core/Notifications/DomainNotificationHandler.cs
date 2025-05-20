using MediatR;

namespace FCG.Domain.Core.Notifications
{
    public class DomainNotificationHandler : INotificationHandler<DomainNotification>
    {
        private List<DomainNotification> _notifications;

        public DomainNotificationHandler()
        {
            _notifications = new List<DomainNotification>();
        }
        public virtual List<DomainNotification> GetNotifications()
        {
            return _notifications;
        }
        public Task Handle(DomainNotification message, CancellationToken cancellationToken)
        {
            _notifications.Add(message);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Erro: {message.Key} - {message.Value}");

            return Task.CompletedTask;
        }
        public bool HasNotifications(string? key = null) // Updated to use nullable reference type
        {
            if (key == null)
                return _notifications.Any();
            else
                return _notifications.Where(x => x.Key.Contains(key)).Any();
        }
        public void ClearNotifications()
        {
            _notifications.Clear();
        }
        public void Dispose()
        {
            _notifications = new List<DomainNotification>();
        }
    }
}
