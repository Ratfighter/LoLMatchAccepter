using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using MatchNotificator.Services;

namespace MatchNotificator.Platforms.Android.Services
{
    public class NotificationService : INotificationService
    {
        const string channelId = "default";
        const string channelName = "Default";
        const string channelDescription = "The default channel for notifications.";

        public const string TitleKey = "title";
        public const string MessageKey = "message";

        int pendingIntentId = 0;
        int messageId = 0;

        private readonly NotificationManagerCompat compatManager;

        public NotificationService()
        {
            CreateNotificationChannel();
            compatManager = NotificationManagerCompat.From(Platform.AppContext);
        }
        public void NotifyUser()
        {
            Show();
        }

        void Show()
        {
            const string title = "LoLMatch";
            const string message = "Match found and accepted!";
            Intent intent = new Intent(Platform.AppContext, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

#pragma warning disable CA1416 // Validate platform compatibility
            var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;
#pragma warning restore CA1416 // Validate platform compatibility

            PendingIntent pendingIntent = PendingIntent.GetActivity(Platform.AppContext, pendingIntentId++, intent, pendingIntentFlags)!;
            NotificationCompat.Builder builder = new NotificationCompat.Builder(Platform.AppContext, channelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetLargeIcon(BitmapFactory.DecodeResource(Platform.AppContext.Resources, Resource.Drawable.abc_ic_menu_overflow_material))
                .SetSmallIcon(Resource.Drawable.abc_ic_menu_overflow_material);

            Notification notification = builder.Build();
            compatManager.Notify(messageId++, notification);
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = channelDescription
                };
                              // Register the channel
                NotificationManager manager = (NotificationManager)Platform.AppContext.GetSystemService(Context.NotificationService)!;
                manager.CreateNotificationChannel(channel);
#pragma warning restore CA1416 // Validate platform compatibility
            }
        }
    }
}
