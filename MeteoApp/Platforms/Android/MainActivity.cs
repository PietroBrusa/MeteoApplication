using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Plugin.Firebase.CloudMessaging;


namespace MeteoApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CreateNotificationChannel();
        HandleIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        HandleIntent(intent);
    }

    // Forwards FCM tap intents to the plugin so notification taps wake the app correctly
    private void HandleIntent(Intent? intent)
    {
        FirebaseCloudMessagingImplementation.OnNewIntent(intent);
    }

    // Required on Android 8+ (API 26): notifications must be posted to a registered channel
    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

        var channelId = $"{PackageName}.general";
        var notificationManager = (NotificationManager?)GetSystemService(NotificationService);
        var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
        notificationManager?.CreateNotificationChannel(channel);

        // Tells the plugin which channel to use when posting incoming push notifications
        FirebaseCloudMessagingImplementation.ChannelId = channelId;
    }
}
