using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using AndroidApp = Android.App;

namespace Notes.Platforms.Android;

public static class EdgeToEdgeHelper
{
    public static void EnableEdgeToEdge(this AndroidApp.Activity activity)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.VanillaIceCream) // API 35+ (Android 15)
        {
            // Use the most modern approach for Android 15+
            // This avoids deprecated setStatusBarColor and setNavigationBarColor
            WindowCompat.SetDecorFitsSystemWindows(activity.Window!, false);
            
            // Ensure proper window flags are set
            if (activity.Window != null)
            {
                activity.Window.SetFlags(WindowManagerFlags.LayoutInScreen | WindowManagerFlags.LayoutInsetDecor, 
                                       WindowManagerFlags.LayoutInScreen | WindowManagerFlags.LayoutInsetDecor);
                
                // Use modern display cutout mode (replaces deprecated LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.P) // API 28+
                {
                    var layoutParams = activity.Window.Attributes;
                    if (layoutParams != null)
                    {
                        layoutParams.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
                        activity.Window.Attributes = layoutParams;
                    }
                }
            }
        }
        else if (Build.VERSION.SdkInt >= BuildVersionCodes.R) // API 30+
        {
            // Fallback for older Android versions
            WindowCompat.SetDecorFitsSystemWindows(activity.Window!, false);
        }
    }
}