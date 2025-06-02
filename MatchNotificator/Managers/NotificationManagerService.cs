using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchNotificator.Services
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
