using Plugin.Firebase.CloudMessaging;
#if ANDROID
using Android.App;
using Android.Content;
using AndroidX.Core.App;
#endif

namespace MeteoApp;

/// <summary>
/// Handles runtime notification permission and exposes the FCM device token.
/// The token is forwarded to the backend so the server can target this device.
/// </summary>
public class NotificationService
{
    const string TokenKey = "fcm_device_token";

    public string? LastError { get; private set; }
    public PermissionStatus LastPermissionStatus { get; private set; } = PermissionStatus.Unknown;

    /// <summary>
    /// Returns the FCM registration token, or null if the user denied permission or registration failed.
    /// Caches the token in <see cref="Preferences"/> so subsequent calls don't always hit the FCM service.
    /// </summary>
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
                System.Diagnostics.Debug.WriteLine($"[FCM] {LastError}");
                return null;
            }

            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                Preferences.Set(TokenKey, token);
                System.Diagnostics.Debug.WriteLine($"[FCM] Device token: {token}");
            }
            else
            {
                LastError = "FCM returned empty token";
                System.Diagnostics.Debug.WriteLine($"[FCM] {LastError}");
            }
            return token;
        }
        catch (Exception ex)
        {
            LastError = $"{ex.GetType().Name}: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[FCM] Token error: {LastError}");
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

    /// <summary>
    /// Posts a local notification on the channel registered by <see cref="MainActivity"/>.
    /// </summary>
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Notification] local notify error: {ex.Message}");
        }
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
