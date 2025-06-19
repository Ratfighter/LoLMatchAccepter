using Android;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace MatchNotificator.Platforms.Android.Permissions
{
    public class NotificationPermission : BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions
        {
            get
            {
                var result = new List<(string androidPermission, bool isRuntime)>();
                if (OperatingSystem.IsAndroidVersionAtLeast(33))
                    result.Add((Manifest.Permission.PostNotifications, true));
                return result.ToArray();
            }
        }
    }
}
