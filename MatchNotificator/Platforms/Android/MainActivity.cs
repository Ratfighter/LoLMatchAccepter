using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using MatchNotificator.Platforms.Android;
using MatchNotificator.Services;

namespace MatchNotificator
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            PermissionStatus status = await Permissions.RequestAsync<NotificationPermission>();
        }
    }
}
