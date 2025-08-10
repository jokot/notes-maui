using Android.App;
using Android.Content.PM;
using Android.OS;
using Notes.Platforms.Android;

namespace Notes;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Enable edge-to-edge display for Android 15+ compatibility
        // This ensures proper display on Android 15 and later versions
        this.EnableEdgeToEdge();
    }
}
