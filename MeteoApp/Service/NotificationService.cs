using Plugin.Firebase.CloudMessaging;
#if ANDROID
using Android.App;
using Android.Content;
using AndroidX.Core.App;
#endif

namespace MeteoApp;

public class NotificationService
{
    const string TokenKey = "fcm_device_token";

    public string? LastError { get; private set; }
    public PermissionStatus LastPermissionStatus { get; private set; } = PermissionStatus.Unknown;

    public async Task<string?> GetDeviceTokenAsync()
    {
        LastError = null;
#if ANDROID
        try
        {
            LastPermissionStatus = await Permissions.RequestAsync<PostNotificationsPermission>();
            if (LastPermissionStatus != PermissionStatus.Granted)
            {
                LastError = $"POST_NOTIFICATIONS not granted (status: {LastPermissionStatus})";
                return null;
            }

            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                Preferences.Set(TokenKey, token);
            }
            else
            {
                LastError = "FCM returned empty token";
            }
            return token;
        }
        catch (Exception ex)
        {
            LastError = $"{ex.GetType().Name}: {ex.Message}";
            return null;
        }
#else
        LastError = "Not running on Android";
        return null;
#endif
    }

    public string? GetCachedToken()
    {
        var cached = Preferences.Get(TokenKey, string.Empty);
        return string.IsNullOrEmpty(cached) ? null : cached;
    }

    public void ShowLocalNotification(string title, string body)
    {
#if ANDROID
        try
        {
            var context = Android.App.Application.Context;
            var channelId = $"{context.PackageName}.general";

            var builder = new NotificationCompat.Builder(context, channelId)
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetPriority((int)NotificationCompat.PriorityDefault)
                .SetAutoCancel(true);

            var manager = NotificationManagerCompat.From(context);
            var notificationId = (int)(DateTime.Now.Ticks % int.MaxValue);
            manager.Notify(notificationId, builder.Build());
        }
        catch (Exception) { }
#endif
    }
}

#if ANDROID
public class PostNotificationsPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new (string, bool)[] { (Android.Manifest.Permission.PostNotifications, true) };
}
#endif
