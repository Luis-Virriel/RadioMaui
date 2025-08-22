using Android.App;
using Android.Content.PM;
using Android.OS;

namespace mauiApp1Prueba.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Inicializar Plugin de huella digital
        Plugin.Fingerprint.CrossFingerprint.SetCurrentActivityResolver(() => this);
    }
}