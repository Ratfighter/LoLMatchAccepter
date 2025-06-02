using MatchNotificator.Services;
using Microsoft.Extensions.Logging;

namespace MatchNotificator
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
#if IOS
            NotificationManagerService.Instance = new Platforms.IOS.Services.NotificationService();
#elif ANDROID
            NotificationManagerService.Instance = new Platforms.Android.Services.NotificationService();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
