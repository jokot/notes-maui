using Android.OS;
using AndroidX.Core.View;
using AndroidApp = Android.App;

namespace Notes.Platforms.Android;

public static class EdgeToEdgeHelper
{
    public static void EnableEdgeToEdge(this AndroidApp.Activity activity)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R) // API 30+
        {
            // Enable edge-to-edge display for Android 15+ compatibility
            WindowCompat.SetDecorFitsSystemWindows(activity.Window!, false);
        }
    }
}