#pragma warning disable CA1416 // Validate platform compatibility
using Microsoft.Extensions.Logging;
using mauiApp1Prueba.Services;
using mauiApp1Prueba.Views;
using mauiApp1Prueba.ViewModels;
#if ANDROID
using Plugin.Fingerprint;
#endif
using Plugin.Maui.Audio; // <- Agregar referencia al AudioManager

namespace mauiApp1Prueba
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });
#if ANDROID
            try
            {
                Plugin.Fingerprint.CrossFingerprint.SetCurrentActivityResolver(() =>
                    Platform.CurrentActivity ?? Android.App.Application.Context as Android.App.Activity);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configurando Plugin.Fingerprint: {ex.Message}");
            }
#endif
            // Inyectar HttpClient manualmente para noticias
            builder.Services.AddSingleton(sp => new System.Net.Http.HttpClient
            {
                BaseAddress = new System.Uri("https://newsdata.io/api/1/")
            });

            // Servicios
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<IBiometricAuthService, BiometricAuthService>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IPhotoService, PhotoService>();
            builder.Services.AddSingleton<IAudioService, AudioService>();

            // Registrar AudioManager de Plugin.Maui.Audio
            builder.Services.AddSingleton<IAudioManager, AudioManager>();

            builder.Services.AddSingleton<AppShell>();
            // Servicio de noticias
            builder.Services.AddSingleton<NewsService>();
            // Servicio de clima
            builder.Services.AddSingleton<WeatherServices>();
            // Servicios para patrocinadores
            builder.Services.AddSingleton<ISponsorService, SponsorService>();
            builder.Services.AddSingleton<IGeolocationService, GeolocationService>();
            // Servicios para cine
            builder.Services.AddSingleton<IMovieService, MovieService>();
            // Servicios del sistema
            builder.Services.AddSingleton<IPreferences>(Preferences.Default);
            builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);

            // Páginas
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<CreateUserPage>();
            builder.Services.AddTransient<EditUserPage>();
            builder.Services.AddTransient<PaginaPatrocinadores>();
            builder.Services.AddTransient<SponsorDetailPage>();
            builder.Services.AddTransient<LocationPickerPage>();
            builder.Services.AddTransient<PaginaCine>();
            builder.Services.AddTransient<PaginaNoticias>();
            builder.Services.AddTransient<TrailerPage>();
            // Agregar las páginas que faltaban
            builder.Services.AddTransient<PaginaClima>();
            builder.Services.AddTransient<PaginaCotizaciones>();
            builder.Services.AddTransient<PaginaPreferencias>();

            // ViewModels
            builder.Services.AddTransient<CreateUserViewModel>();
            builder.Services.AddTransient<RadioHomeViewModel>(); // <- Este ahora puede recibir IAudioManager
            builder.Services.AddTransient<PatrocinadoresViewModel>();
            builder.Services.AddTransient<SponsorDetailViewModel>();
            builder.Services.AddTransient<LocationPickerViewModel>();
            builder.Services.AddTransient<PaginaCineViewModel>();
            builder.Services.AddTransient<NewsViewModel>();
            builder.Services.AddTransient<TrailerPageViewModel>();
            // Agregar los ViewModels que faltaban (si los tienes)
            // builder.Services.AddTransient<ClimaViewModel>();
            // builder.Services.AddTransient<CotizacionesViewModel>();
            // builder.Services.AddTransient<PreferenciasViewModel>();

#if DEBUG
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif
            return builder.Build();
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
