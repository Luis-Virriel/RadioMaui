#pragma warning disable CA1416 // Validate platform compatibility

using Microsoft.Extensions.Logging;
using mauiApp1Prueba.Services;
using mauiApp1Prueba.Views;
using mauiApp1Prueba.ViewModels;

// Solo incluir ViewModels si los estás usando
// using mauiApp1Prueba.ViewModels;

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
                    // Solo incluir fuentes que realmente existen
                    // fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
                });

            // Servicios de datos (Singleton = una instancia para toda la app)
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<IBiometricAuthService, BiometricAuthService>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IPhotoService, PhotoService>();

            // Servicios del sistema
            builder.Services.AddSingleton<IPreferences>(Preferences.Default);
            builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);

            // Páginas (Transient = nueva instancia cada vez)
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<CreateUserPage>();

            // ViewModels
            builder.Services.AddTransient<CreateUserViewModel>();

#if DEBUG
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            return builder.Build();
        }
    }
}

#pragma warning restore CA1416 // Validate platform compatibility