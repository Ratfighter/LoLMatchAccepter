using MatchNotificator.Services;

namespace MatchNotificator.Managers
{
    public static class NotificationManagerService
    {
        private static INotificationService? _instance = null;
        public static INotificationService Instance
        {
            get => _instance ?? throw new InvalidOperationException("Notification service is not initialized.");
            set => _instance = value ?? throw new ArgumentNullException(nameof(value), "Notification service cannot be null.");
        }
    }
}
