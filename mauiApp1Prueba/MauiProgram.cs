#pragma warning disable CA1416 // Validate platform compatibility

using Microsoft.Extensions.Logging;
using mauiApp1Prueba.Services;
using mauiApp1Prueba.Views;
using mauiApp1Prueba.ViewModels;

#if ANDROID
using Plugin.Fingerprint;
#endif

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

            // Configurar Plugin.Fingerprint para Android
#if ANDROID
            try
            {
                CrossFingerprint.SetCurrentActivityResolver(() =>
                    Platform.CurrentActivity ?? Android.App.Application.Context as Android.App.Activity);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configurando Plugin.Fingerprint: {ex.Message}");
            }
#endif

            // Servicios de datos existentes
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<IBiometricAuthService, BiometricAuthService>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IPhotoService, PhotoService>();
            builder.Services.AddSingleton<IAudioService, AudioService>();
            builder.Services.AddSingleton<AppShell>();

            // Servicio de clima
            builder.Services.AddSingleton<WeatherServices>();

            // 👉 SERVICIOS PARA PATROCINADORES
            builder.Services.AddSingleton<ISponsorService, SponsorService>();
            builder.Services.AddSingleton<IGeolocationService, GeolocationService>();

            // 🎬 SERVICIOS PARA CINE - Sin dependencias adicionales
            builder.Services.AddSingleton<IMovieService, MovieService>();

            // Servicios del sistema
            builder.Services.AddSingleton<IPreferences>(Preferences.Default);
            builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);

            // Páginas existentes
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<CreateUserPage>();

            // 👉 PÁGINAS PARA PATROCINADORES
            builder.Services.AddTransient<PaginaPatrocinadores>(); // Tu página existente
            builder.Services.AddTransient<SponsorDetailPage>();  // Nueva página de detalles
            builder.Services.AddTransient<LocationPickerPage>(); // Selector de mapa
            // builder.Services.AddTransient<SponsorMapPage>();     // TODO: Crear esta página

            // 🎬 PÁGINA PARA CINE
            builder.Services.AddTransient<PaginaCine>();

            // ViewModels existentes
            builder.Services.AddTransient<CreateUserViewModel>();
            builder.Services.AddTransient<RadioHomeViewModel>();

            // 👉 VIEWMODELS PARA PATROCINADORES
            builder.Services.AddTransient<PatrocinadoresViewModel>();
            builder.Services.AddTransient<SponsorDetailViewModel>(); // Nuevo ViewModel de detalles
            builder.Services.AddTransient<LocationPickerViewModel>(); // ViewModel del selector de mapa
            // builder.Services.AddTransient<SponsorMapViewModel>();    // TODO: Crear este ViewModel

            // 🎬 VIEWMODEL PARA CINE
            builder.Services.AddTransient<PaginaCineViewModel>();

#if DEBUG
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            return builder.Build();
        }
    }
}

#pragma warning restore CA1416 // Validate platform compatibility