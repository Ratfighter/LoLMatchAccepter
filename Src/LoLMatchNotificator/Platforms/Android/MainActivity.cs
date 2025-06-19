using Android.App;
using Android.Content.PM;
using Android.OS;
using MatchNotificator.Platforms.Android.Permissions;

namespace MatchNotificator.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _ = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<NotificationPermission>();
        }
    }
}
